// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Collections;

namespace FastOSC;

public interface IOSCPacket;

public record OSCBundle : IOSCPacket, IEnumerable<IOSCPacket>
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

    public IEnumerator<IOSCPacket> GetEnumerator() => (IEnumerator<IOSCPacket>)Packets.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public record OSCMessage : IOSCPacket, IEnumerable<object>
{
    public readonly string Address;
    public readonly object[] Arguments;

    internal readonly int TypeTagsLength;
    internal readonly int ArgumentsLength;

    public OSCMessage(string address, params object[] arguments)
    {
        OSCValidation.ThrowIfInvalidAddress(address);
        ArgumentOutOfRangeException.ThrowIfZero(arguments.Length, nameof(arguments));

        Address = address;
        Arguments = arguments;

        OSCEncoder.CalculateLengths(Arguments, ref TypeTagsLength, ref ArgumentsLength);
        TypeTagsLength = OSCUtils.Align(TypeTagsLength + 2); // +2 for comma + terminator
    }

    internal OSCMessage(string address, object[] arguments, int typeTagsLength, int argumentsLength)
    {
        OSCValidation.ThrowIfInvalidAddress(address);
        ArgumentOutOfRangeException.ThrowIfZero(arguments.Length, nameof(arguments));

        Address = address;
        Arguments = arguments;

        TypeTagsLength = typeTagsLength;
        ArgumentsLength = argumentsLength;
    }

    public IEnumerator<object> GetEnumerator() => (IEnumerator<object>)Arguments.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}