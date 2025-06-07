// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public interface IOSCPacket;

public record OSCBundle : IOSCPacket
{
    public readonly OSCTimeTag TimeTag;
    public readonly IOSCPacket[] Packets;

    public OSCBundle(OSCTimeTag timeTag, params IOSCPacket[] packets)
    {
        TimeTag = timeTag;
        Packets = packets;
    }
}

public record OSCMessage : IOSCPacket
{
    public readonly string Address;
    public readonly object?[] Arguments;

    public OSCMessage(string address, params object?[] arguments)
    {
        if (string.IsNullOrEmpty(address)) throw new InvalidOperationException($"{nameof(address)} must be non-null and have a non-zero length");
        if (arguments.Length == 0) throw new InvalidOperationException($"{nameof(arguments)} must have a non-zero length");

        Address = address;
        Arguments = arguments;
    }
}