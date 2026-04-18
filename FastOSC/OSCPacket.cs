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
        ArgumentOutOfRangeException.ThrowIfZero(packets.Length, nameof(packets));

        TimeTag = timeTag;
        Packets = packets;
    }

    public OSCBundle(DateTime dateTime, params IOSCPacket[] packets)
    {
        ArgumentOutOfRangeException.ThrowIfZero(packets.Length, nameof(packets));

        TimeTag = new OSCTimeTag(dateTime);
        Packets = packets;
    }
}

public record OSCMessage : IOSCPacket
{
    public readonly string Address;
    public readonly object[] Arguments;

    public OSCMessage(string address, params object[] arguments)
    {
        OSCValidation.ThrowIfInvalidAddress(address);
        ArgumentOutOfRangeException.ThrowIfZero(arguments.Length, nameof(arguments));

        Address = address;
        Arguments = arguments;
    }
}