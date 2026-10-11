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

        receiver.OnPacketReceived += raisePacketReceivedAsync;
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

    public Task EnableSendAsync() => sender.ConnectAsync(SendEndpoint);
    public void EnableReceive() => receiver.Connect(ReceiveEndpoint);

    public void DisableSend() => sender.Disconnect();
    public Task DisableReceiveAsync() => receiver.DisconnectAsync();

    public Task SendPacketAsync(IOSCPacket packet, CancellationToken token = default) => sender.SendAsync(packet, token);
}