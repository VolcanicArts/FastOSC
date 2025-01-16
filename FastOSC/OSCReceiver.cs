// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace FastOSC;

public class OSCReceiver
{
    private readonly byte[] buffer = new byte[4096];
    private Socket? socket;
    private CancellationTokenSource? tokenSource;
    private Task? receivingTask;

    public Action<OSCMessage>? OnMessageReceived;
    public Action<OSCBundle>? OnBundleReceived;

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
            await receivingTask.ConfigureAwait(false);
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

    private async void runReceiveLoop()
    {
        Debug.Assert(tokenSource is not null);

        while (!tokenSource.Token.IsCancellationRequested)
        {
            try
            {
                Array.Clear(buffer, 0, buffer.Length);
                await socket!.ReceiveAsync(buffer, SocketFlags.None, tokenSource.Token);

                var packet = OSCDecoder.Decode(buffer);
                if (!packet.IsValid) continue;

                if (packet.IsBundle)
                    OnBundleReceived?.Invoke(packet.AsBundle());
                else
                    OnMessageReceived?.Invoke(packet.AsMessage());
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}