// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeMethodOrOperatorBody

namespace FastOSC;

public static class OSCDecoder
{
    private static readonly Encoding encoding = Encoding.UTF8;

    public static bool TryDecode(ReadOnlySpan<byte> data, [NotNullWhen(true)] out IOSCPacket? packet)
    {
        packet = Decode(data);
        return packet is not null;
    }

    public static IOSCPacket? Decode(ReadOnlySpan<byte> data)
    {
        var index = 0;
        return decode(data, ref index);
    }

    private static IOSCPacket? decode(ReadOnlySpan<byte> data, ref int index)
    {
        return data.Slice(index, 8).SequenceEqual("#bundle\0"u8) ? decodeBundle(data, ref index) : decodeMessage(data, ref index);
    }

    #region Bundle

    private static OSCBundle decodeBundle(ReadOnlySpan<byte> data, ref int index)
    {
        index += 8; // header

        var timeTag = decodeTimeTag(data, ref index);

        var packetIndex = index;
        var packetCount = 0;

        while (packetIndex < data.Length)
        {
            var packetLength = decodeInt(data, ref packetIndex);
            packetIndex += packetLength;
            packetCount++;
        }

        var packets = new IOSCPacket[packetCount];

        for (var i = 0; i < packetCount; i++)
        {
            var packetLength = decodeInt(data, ref index);
            var packet = Decode(data.Slice(index, packetLength));
            index += packetLength;

            packets[i] = packet switch
            {
                OSCMessage message => message,
                OSCBundle bundle => bundle,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return new OSCBundle(timeTag, packets);
    }

    #endregion

    #region Message

    private static OSCMessage? decodeMessage(ReadOnlySpan<byte> data, ref int index)
    {
        var address = decodeAddress(data, ref index);
        if (address is null) return null;

        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based

        var typeTags = decodeTypeTags(data, ref index);
        if (typeTags.IsEmpty) return null;

        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based

        var values = decodeArguments(typeTags, data, ref index);
        return new OSCMessage(address, values);
    }

    private static string? decodeAddress(ReadOnlySpan<byte> data, ref int index)
    {
        if (data[index] != OSCConst.SLASH) return null;

        var start = index;
        index = OSCUtils.FindByteIndex(data, index);
        return encoding.GetString(data.Slice(start, index - start));
    }

    private static ReadOnlySpan<byte> decodeTypeTags(ReadOnlySpan<byte> data, ref int index)
    {
        if (data[index] != OSCConst.COMMA) return ReadOnlySpan<byte>.Empty;

        var start = index;
        index = OSCUtils.FindByteIndex(data, index);
        return data.Slice(start + 1, index - (start + 1));
    }

    private static object?[] decodeArguments(ReadOnlySpan<byte> typeTags, ReadOnlySpan<byte> data, ref int index)
    {
        var values = new object?[calculateArgumentsLength(typeTags)];
        var valueIndex = 0;

        for (var i = 0; i < typeTags.Length; i++)
        {
            var type = typeTags[i];

            if (type == OSCConst.ARRAY_BEGIN)
            {
                var internalArrayValues = decodeInternalArray(typeTags, i, data, ref index);
                values[valueIndex++] = internalArrayValues;
                i += internalArrayValues.Length + 1;
                continue;
            }

            values[valueIndex++] = type switch
            {
                OSCConst.STRING or OSCConst.ALT_STRING => decodeString(data, ref index),
                OSCConst.INT => decodeInt(data, ref index),
                OSCConst.INFINITY => float.PositiveInfinity,
                OSCConst.FLOAT => decodeFloat(data, ref index),
                OSCConst.TRUE => true,
                OSCConst.FALSE => false,
                OSCConst.BLOB => decodeByteArray(data, ref index),
                OSCConst.LONG => decodeLong(data, ref index),
                OSCConst.DOUBLE => decodeDouble(data, ref index),
                OSCConst.CHAR => decodeChar(data, ref index),
                OSCConst.NIL => null,
                OSCConst.RGBA => decodeRGBA(data, ref index),
                OSCConst.MIDI => decodeMidi(data, ref index),
                OSCConst.TIMETAG => decodeTimeTag(data, ref index),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return values;
    }

    private static int calculateArgumentsLength(ReadOnlySpan<byte> typeTags)
    {
        var length = 0;
        var i = 0;

        while (i < typeTags.Length)
        {
            if (typeTags[i] == OSCConst.ARRAY_BEGIN)
            {
                length++;
                var nestedLevel = 1;

                i++;

                while (i < typeTags.Length && nestedLevel > 0)
                {
                    switch (typeTags[i])
                    {
                        case OSCConst.ARRAY_BEGIN:
                            nestedLevel++;
                            break;

                        case OSCConst.ARRAY_END:
                            nestedLevel--;
                            break;
                    }

                    i++;
                }
            }
            else
            {
                length++;
                i++;
            }
        }

        return length;
    }

    private static object?[] decodeInternalArray(ReadOnlySpan<byte> typeTags, int typeTagIndex, ReadOnlySpan<byte> data, ref int index)
    {
        var arrayEndIndex = OSCUtils.FindByteIndex(typeTags, typeTagIndex, OSCConst.ARRAY_END);
        return decodeArguments(typeTags[(typeTagIndex + 1)..arrayEndIndex], data, ref index);
    }

    private static int decodeInt(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(data.Slice(index, 4));
        index += 4;
        return value;
    }

    private static float decodeFloat(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadSingleBigEndian(data.Slice(index, 4));
        index += 4;
        return value;
    }

    private static string decodeString(ReadOnlySpan<byte> data, ref int index)
    {
        var start = index;
        index = OSCUtils.FindByteIndex(data, index);

        var stringData = encoding.GetString(data.Slice(start, index - start));
        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based
        return stringData;
    }

    private static byte[] decodeByteArray(ReadOnlySpan<byte> data, ref int index)
    {
        var length = decodeInt(data, ref index);
        var byteArray = data.Slice(index, length).ToArray();
        index += OSCUtils.Align(length + 1); // +1 for null terminator
        return byteArray;
    }

    private static long decodeLong(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt64BigEndian(data.Slice(index, 8));
        index += 8;
        return value;
    }

    private static double decodeDouble(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadDoubleBigEndian(data.Slice(index, 8));
        index += 8;
        return value;
    }

    private static char decodeChar(ReadOnlySpan<byte> data, ref int index)
    {
        var values = encoding.GetChars(data.Slice(index, 4).ToArray());
        index += 4;
        return values[^1];
    }

    private static OSCRGBA decodeRGBA(ReadOnlySpan<byte> data, ref int index)
    {
        var r = data[index++];
        var g = data[index++];
        var b = data[index++];
        var a = data[index++];

        return new OSCRGBA(r, g, b, a);
    }

    private static OSCMidi decodeMidi(ReadOnlySpan<byte> data, ref int index)
    {
        var portId = data[index++];
        var status = data[index++];
        var data1 = data[index++];
        var data2 = data[index++];

        return new OSCMidi(portId, status, data1, data2);
    }

    private static OSCTimeTag decodeTimeTag(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadUInt64BigEndian(data.Slice(index, 8));
        index += 8;
        return new OSCTimeTag(value);
    }

    #endregion
}