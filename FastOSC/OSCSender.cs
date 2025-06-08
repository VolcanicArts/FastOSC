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
        if (socket is not null) throw new InvalidOperationException($"Please call {nameof(Disconnect)} first");

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            await socket.ConnectAsync(endPoint);
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
        if (socket is null) throw new InvalidOperationException($"Please call {nameof(ConnectAsync)} first");

        try
        {
            socket.Close();
        }
        finally
        {
            socket.Dispose();
            socket = null;
        }
    }

    public void Send(OSCMessage message)
    {
        if (socket is null || !socket.Connected) throw new InvalidOperationException($"Please call {nameof(ConnectAsync)} first");

        var data = OSCEncoder.Encode(message);
        socket.Send(data);
    }
}