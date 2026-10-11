// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;

namespace FastOSC;

public class OSCSender
{
    private Socket? socket;

    public async Task ConnectAsync(IPEndPoint endPoint)
    {
        if (socket is not null)
            throw new InvalidOperationException($"Please call {nameof(Disconnect)} first");

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            await socket.ConnectAsync(endPoint).ConfigureAwait(false);
        }
        catch (Exception)
        {
            socket.Dispose();
            socket = null;
            throw;
        }
    }

    public void Disconnect()
    {
        if (socket is null)
            throw new InvalidOperationException($"Please call {nameof(ConnectAsync)} first");

        socket.Dispose();
        socket = null;
    }

    public async Task SendAsync(IOSCPacket packet, CancellationToken token = default)
    {
        if (socket is null || !socket.Connected)
            throw new InvalidOperationException($"Please call {nameof(ConnectAsync)} first");

        switch (packet)
        {
            case OSCMessage message:
            {
                var data = OSCEncoder.Encode(message);
                await socket.SendAsync(data, token).ConfigureAwait(false);
                break;
            }

            case OSCBundle bundle:
            {
                var data = OSCEncoder.Encode(bundle);
                await socket.SendAsync(data, token).ConfigureAwait(false);
                break;
            }
        }
    }

    public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
    {
        if (socket is null || !socket.Connected)
            throw new InvalidOperationException($"Please call {nameof(ConnectAsync)} first");

        await socket.SendAsync(data, token).ConfigureAwait(false);
    }
}