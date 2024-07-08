// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;

namespace FastOSC;

public class OSCSender
{
    private Socket? socket;

    public async Task ConnectAsync(IPEndPoint endPoint)
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        await socket.ConnectAsync(endPoint);
    }

    public async Task DisconnectAsync()
    {
        if (socket is null) throw new InvalidOperationException($"{nameof(OSCSender)} must be connected before it can be disconnected");

        await socket.DisconnectAsync(false);
        socket.Dispose();
        socket = null;
    }

    public void Send(OSCMessage message)
    {
        if (socket is null || !socket.Connected) throw new InvalidOperationException($"{nameof(OSCSender)} needs to be enabled before sending data");

        var data = OSCEncoder.Encode(message);

        if (socket?.Connected ?? false)
            socket?.Send(data);
    }
}
