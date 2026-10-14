// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;

namespace FastOSC;

/// <summary>
/// A sender that takes <see cref="IOSCPacket"/>s and encodes them into bytes to send
/// </summary>
public class OSCSender
{
    private Socket? socket;

    public void Connect(IPEndPoint endPoint)
    {
        if (socket is not null)
            throw new InvalidOperationException($"Please call {nameof(Disconnect)} first");

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Connect(endPoint);
        }
        catch
        {
            socket.Dispose();
            socket = null;
            throw;
        }
    }

    public void Disconnect()
    {
        if (socket is null)
            throw new InvalidOperationException($"Please call {nameof(Connect)} first");

        socket.Dispose();
        socket = null;
    }

    public Task SendPacketAsync(IOSCPacket packet, CancellationToken token = default)
    {
        var data = OSCEncoder.Encode(packet);
        return SendDataAsync(data, token);
    }

    public async Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
    {
        if (socket is null || !socket.Connected)
            throw new InvalidOperationException($"Please call {nameof(Connect)} first");

        await socket.SendAsync(data, token).ConfigureAwait(false);
    }
}