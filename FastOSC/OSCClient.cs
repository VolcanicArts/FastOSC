// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Net;

namespace FastOSC;

/// <summary>
/// A wrapper class around an <see cref="OSCSender"/> and <see cref="OSCReceiver"/>
/// </summary>
/// <remarks><see cref="OnPacketReceived"/> is processed synchronously in the order of additions</remarks>
public class OSCClient
{
    private readonly OSCSender sender = new();
    private readonly OSCReceiver receiver = new();

    public IPEndPoint SendEndpoint { get; }
    public IPEndPoint ReceiveEndpoint { get; }

    public event Func<IOSCPacket, Task>? OnPacketReceived
    {
        add => receiver.OnPacketReceived += value;
        remove => receiver.OnPacketReceived -= value;
    }

    public OSCClient(IPEndPoint sendEndpoint, IPEndPoint receiveEndpoint)
    {
        SendEndpoint = sendEndpoint;
        ReceiveEndpoint = receiveEndpoint;
    }

    public void ConnectSend() => sender.Connect(SendEndpoint);
    public void ConnectReceive() => receiver.Connect(ReceiveEndpoint);

    public void DisconnectSend() => sender.Disconnect();
    public Task DisconnectReceiveAsync() => receiver.DisconnectAsync();

    public Task SendPacketAsync(IOSCPacket packet, CancellationToken token = default) => sender.SendPacketAsync(packet, token);
    public Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken token = default) => sender.SendDataAsync(data, token);
}