// Copyright (c) VolcanicArts. Licensed under the LGPL License.
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

    public OSCBundle(DateTime dateTime, params IOSCPacket[] packets)
    {
        TimeTag = new OSCTimeTag(dateTime);
        Packets = packets;
    }
}

public record OSCMessage : IOSCPacket
{
    public readonly string Address;
    public readonly object?[] Arguments;

    public OSCMessage(string address, params object?[] arguments)
    {
        if (string.IsNullOrWhiteSpace(address)) throw new InvalidOperationException($"{nameof(address)} must be a non-null, non-zero length, and non-whitespace string");
        if (arguments.Length == 0) throw new InvalidOperationException($"{nameof(arguments)} must have a non-zero length");

        Address = address;
        Arguments = arguments;
    }
}