// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace FastOSC;

public class OSCReceiver
{
    private readonly byte[] buffer;
    private Socket? socket;
    private CancellationTokenSource? tokenSource;
    private Task? receivingTask;

    public Action<IOSCPacket>? OnPacketReceived;

    public OSCReceiver(int bufferSize = 256)
    {
        buffer = new byte[bufferSize];
    }

    public void Connect(IPEndPoint endPoint)
    {
        if (socket is not null || tokenSource is not null || receivingTask is not null) throw new InvalidOperationException($"Please call {nameof(DisconnectAsync)} first");

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Bind(endPoint);

            if (!socket.IsBound)
                throw new InvalidOperationException("Socket failed to bind.");

            tokenSource = new CancellationTokenSource();
            receivingTask = Task.Run(runReceiveLoop, tokenSource.Token);
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
        if (socket is null || tokenSource is null || receivingTask is null) throw new InvalidOperationException($"Please call {nameof(Connect)} first");

        await tokenSource.CancelAsync();

        try
        {
            await receivingTask;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            receivingTask.Dispose();
            receivingTask = null;

            tokenSource.Dispose();
            tokenSource = null;
        }

        try
        {
            socket.Shutdown(SocketShutdown.Receive);
        }
        finally
        {
            socket.Close();
            socket = null;
        }
    }

    private async Task runReceiveLoop()
    {
        Debug.Assert(tokenSource is not null);
        Debug.Assert(socket is not null);

        Array.Clear(buffer, 0, buffer.Length);

        while (!tokenSource.IsCancellationRequested)
        {
            try
            {
                await socket.ReceiveAsync(buffer, SocketFlags.None, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var packet = OSCDecoder.Decode(buffer);
            Array.Clear(buffer, 0, buffer.Length);

            if (packet is not null) OnPacketReceived?.Invoke(packet);
        }
    }
}