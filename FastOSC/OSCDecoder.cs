﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastOSC;

public static class OSCDecoder
{
    private static Encoding encoding = Encoding.ASCII;

    /// <summary>
    /// This can be used to override the encoding.
    /// For example, some senders may support UTF8 strings.
    /// </summary>
    public static void SetEncoding(Encoding encoding)
    {
        OSCDecoder.encoding = encoding;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static OSCPacket Decode(byte[] data)
    {
        var index = 0;
        return decode(data, ref index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static OSCPacket decode(byte[] data, ref int index)
    {
        return (char)data[index] == '#' ? new OSCPacket(decodeBundle(data, ref index)) : new OSCPacket(decodeMessage(data, ref index));
    }

    #region Bundle

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static OSCBundle decodeBundle(byte[] data, ref int index)
    {
        index += 8; // header

        var timeTag = decodeTimeTag(data, ref index);

        var elements = new List<IOSCElement>();

        while (index < data.Length)
        {
            //_ = decodeInt(data, ref index);
            index += 4; // element length
            var elementPacket = decode(data, ref index);
            if (!elementPacket.IsValid) continue;

            if (elementPacket.IsBundle)
                elements.Add(elementPacket.AsBundle());
            else
                elements.Add(elementPacket.AsMessage());
        }

        return new OSCBundle(timeTag, elements.ToArray());
    }

    #endregion

    #region Message

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static OSCMessage? decodeMessage(byte[] data, ref int index)
    {
        var address = decodeAddress(data, ref index);

        if (address is null)
            return null;

        index = OSCUtils.Align(index);

        var typeTags = decodeTypeTags(data, ref index);

        if (typeTags.Length == 0)
            return null;

        index = OSCUtils.Align(index);

        var values = decodeAttributes(typeTags, data, ref index);

        return new OSCMessage(address, values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static string? decodeAddress(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OSCChars.SLASH) return null;

        index = OSCUtils.FindByteIndex(data, index);
        return encoding.GetString(data.AsSpan(start, index - start));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Span<byte> decodeTypeTags(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OSCChars.COMMA) return Array.Empty<byte>();

        index = OSCUtils.FindByteIndex(data, index);
        return data.AsSpan(start + 1, index - (start + 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static object?[] decodeAttributes(Span<byte> typeTags, byte[] data, ref int index)
    {
        var values = new object?[calculateValueArrayLength(typeTags)];

        var valueIndex = 0;

        for (var i = 0; i < typeTags.Length; i++)
        {
            var type = typeTags[i];

            if (type == OSCChars.ARRAY_BEGIN)
            {
                var internalArrayValues = decodeInternalArray(typeTags, i, data, ref index);
                values[valueIndex++] = internalArrayValues;
                i += internalArrayValues.Length + 1;
                continue;
            }

            values[valueIndex++] = type switch
            {
                OSCChars.STRING or OSCChars.ALT_STRING => decodeString(data, ref index),
                OSCChars.INT => decodeInt(data, ref index),
                OSCChars.INFINITY => float.PositiveInfinity,
                OSCChars.FLOAT => decodeFloat(data, ref index),
                OSCChars.TRUE => true,
                OSCChars.FALSE => false,
                OSCChars.BLOB => decodeByteArray(data, ref index),
                OSCChars.LONG => decodeLong(data, ref index),
                OSCChars.DOUBLE => decodeDouble(data, ref index),
                OSCChars.CHAR => decodeChar(data, ref index),
                OSCChars.NIL => null,
                OSCChars.RGBA => decodeRGBA(data, ref index),
                OSCChars.MIDI => decodeMidi(data, ref index),
                OSCChars.TIMETAG => decodeTimeTag(data, ref index),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int calculateValueArrayLength(Span<byte> typeTags)
    {
        var length = 0;
        var i = 0;

        while (i < typeTags.Length)
        {
            if (typeTags[i] == OSCChars.ARRAY_BEGIN)
            {
                length++;
                var nestedLevel = 1;

                i++;

                while (i < typeTags.Length && nestedLevel > 0)
                {
                    switch (typeTags[i])
                    {
                        case OSCChars.ARRAY_BEGIN:
                            nestedLevel++;
                            break;

                        case OSCChars.ARRAY_END:
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static object?[] decodeInternalArray(Span<byte> typeTags, int typeTagIndex, byte[] data, ref int index)
    {
        var arrayEndIndex = OSCUtils.FindByteIndex(typeTags, typeTagIndex, OSCChars.ARRAY_END);
        var internalAttributes = decodeAttributes(typeTags.ToArray().AsSpan((typeTagIndex + 1)..arrayEndIndex), data, ref index);
        return internalAttributes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int decodeInt(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(index, 4));
        index += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static float decodeFloat(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadSingleBigEndian(data.AsSpan(index, 4));
        index += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static string decodeString(byte[] data, ref int index)
    {
        var start = index;
        index = OSCUtils.FindByteIndex(data, index);

        var stringData = encoding.GetString(data.AsSpan(start, index - start));
        index = OSCUtils.Align(index);
        return stringData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static byte[] decodeByteArray(byte[] data, ref int index)
    {
        var length = decodeInt(data, ref index);
        var byteArray = data[index..(index + length)];
        index += OSCUtils.Align(length, false);
        return byteArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static long decodeLong(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt64BigEndian(data.AsSpan(index, 8));
        index += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static double decodeDouble(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadDoubleBigEndian(data.AsSpan(index, 8));
        index += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static char decodeChar(byte[] data, ref int index)
    {
        var values = encoding.GetChars(data[index..(index + 4)]);
        index += 4;
        return values[^1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static OSCRGBA decodeRGBA(byte[] data, ref int index)
    {
        var r = data[index++];
        var g = data[index++];
        var b = data[index++];
        var a = data[index++];

        return new OSCRGBA(r, g, b, a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static OSCMidi decodeMidi(byte[] data, ref int index)
    {
        var portId = data[index++];
        var status = data[index++];
        var data1 = data[index++];
        var data2 = data[index++];

        return new OSCMidi(portId, status, data1, data2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static OSCTimeTag decodeTimeTag(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(index, 8));
        index += 8;
        return new OSCTimeTag(value);
    }

    #endregion
}
