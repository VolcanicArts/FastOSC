// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace FastOSC;

/// <summary>
/// A receiver that takes bytes and decodes them into <see cref="IOSCPacket"/>s
/// </summary>
/// <remarks><see cref="OnPacketReceived"/> is processed synchronously in the order of additions</remarks>
public class OSCReceiver
{
    private readonly byte[] buffer;
    private Socket? socket;
    private CancellationTokenSource? tokenSource;
    private Task? receivingTask;

    public Func<IOSCPacket, Task>? OnPacketReceived;

    public OSCReceiver(int bufferSize = 256)
    {
        buffer = new byte[bufferSize];
    }

    public void Connect(IPEndPoint endPoint)
    {
        if (socket is not null || tokenSource is not null || receivingTask is not null)
            throw new InvalidOperationException($"Please call {nameof(DisconnectAsync)} first");

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Bind(endPoint);

            if (!socket.IsBound)
                throw new InvalidOperationException("Socket failed to bind.");

            tokenSource = new CancellationTokenSource();
            receivingTask = runReceiveLoop();
        }
        catch
        {
            socket?.Dispose();
            socket = null;
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (socket is null || tokenSource is null || receivingTask is null)
            throw new InvalidOperationException($"Please call {nameof(Connect)} first");

        await tokenSource.CancelAsync().ConfigureAwait(false);

        try
        {
            await receivingTask.ConfigureAwait(false);
        }
        finally
        {
            socket.Dispose();
            tokenSource.Dispose();

            receivingTask = null;
            tokenSource = null;
            socket = null;
        }
    }

    private async Task runReceiveLoop()
    {
        Debug.Assert(socket is not null);
        Debug.Assert(tokenSource is not null);

        while (!tokenSource.IsCancellationRequested)
        {
            try
            {
                var receivedBytes = await socket.ReceiveAsync(buffer, SocketFlags.None, tokenSource.Token).ConfigureAwait(false);
                if (receivedBytes == 0) continue;

                var packet = OSCDecoder.Decode(buffer.AsSpan(0, receivedBytes));
                if (packet is null) continue;

                await raisePacketReceivedAsync(packet).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException) when (tokenSource.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task raisePacketReceivedAsync(IOSCPacket packet)
    {
        var handlers = OnPacketReceived;
        if (handlers is null) return;

        foreach (var handler in handlers.GetInvocationList().Cast<Func<IOSCPacket, Task>>())
        {
            await handler(packet).ConfigureAwait(false);
        }
    }
}