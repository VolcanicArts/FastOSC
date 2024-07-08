// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

    public void Enable(IPEndPoint endPoint)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(endPoint);

        if (!socket.IsBound)
        {
            throw new InvalidOperationException("Socket failed to bind");
        }

        tokenSource = new CancellationTokenSource();
        receivingTask = Task.Run(runReceiveLoop, tokenSource.Token);
    }

    public async Task DisableAsync()
    {
        if (tokenSource is not null)
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = null;
        }

        if (receivingTask is not null)
            await receivingTask;

        receivingTask?.Dispose();
        socket?.Close();

        receivingTask = null;
        socket = null;
    }

    private async void runReceiveLoop()
    {
        while (!tokenSource!.Token.IsCancellationRequested)
        {
            try
            {
                Array.Clear(buffer, 0, buffer.Length);
                await socket!.ReceiveAsync(buffer, SocketFlags.None, tokenSource.Token);

                var message = OSCDecoder.Decode(buffer);
                if (message is null) continue;

                OnMessageReceived?.Invoke(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
