// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Net;

namespace FastOSC;

public class OSCClient
{
    public event Func<IOSCPacket, Task>? OnPacketReceived;

    private readonly OSCSender sender = new();
    private readonly OSCReceiver receiver = new();

    public IPEndPoint SendEndpoint { get; }
    public IPEndPoint ReceiveEndpoint { get; }

    public OSCClient(IPEndPoint sendEndpoint, IPEndPoint receiveEndpoint)
    {
        SendEndpoint = sendEndpoint;
        ReceiveEndpoint = receiveEndpoint;

        receiver.OnPacketReceived += packet => OnPacketReceived?.Invoke(packet) ?? Task.CompletedTask;
    }

    public Task EnableSend() => sender.ConnectAsync(SendEndpoint);
    public void EnableReceive() => receiver.Connect(ReceiveEndpoint);

    public void DisableSend() => sender.Disconnect();
    public Task DisableReceive() => receiver.DisconnectAsync();

    public Task SendMessage(OSCMessage message) => sender.Send(message);

    private Task sendBundle(OSCBundle bundle) => sender.Send(bundle);
    public Task SendBundle(OSCTimeTag timeTag, params IOSCPacket[] values) => sendBundle(new OSCBundle(timeTag, values));
    public Task SendBundle(DateTime dateTime, params IOSCPacket[] values) => sendBundle(new OSCBundle(dateTime, values));
}