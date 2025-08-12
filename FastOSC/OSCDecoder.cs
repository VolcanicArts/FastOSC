// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeMethodOrOperatorBody

namespace FastOSC;

public static class OSCDecoder
{
    private static readonly UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: false);

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

        var timeTag = readTimeTag(data, ref index);

        var packetIndex = index;
        var packetCount = 0;

        while (packetIndex < data.Length)
        {
            var packetLength = readInt(data, ref packetIndex);
            packetIndex += packetLength;
            packetCount++;
        }

        var packets = new IOSCPacket[packetCount];

        for (var i = 0; i < packetCount; i++)
        {
            var packetLength = readInt(data, ref index);
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
        var address = readAddress(data, ref index);
        if (string.IsNullOrEmpty(address)) return null;

        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based

        var typeTags = readTypeTags(data, ref index);
        if (typeTags.IsEmpty) return null;

        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based

        var values = readArguments(typeTags, data, ref index);
        return new OSCMessage(address, values);
    }

    private static string? readAddress(ReadOnlySpan<byte> data, ref int index)
    {
        if (data[index] != OSCConst.SLASH) return null;

        var start = index;
        index = OSCUtils.FindNullTerminator(data, index);
        return encoding.GetString(data.Slice(start, index - start));
    }

    private static ReadOnlySpan<byte> readTypeTags(ReadOnlySpan<byte> data, ref int index)
    {
        if (data[index] != OSCConst.COMMA) return ReadOnlySpan<byte>.Empty;

        var start = index;
        index = OSCUtils.FindNullTerminator(data, index);
        return data.Slice(start + 1, index - (start + 1));
    }

    private static object?[] readArguments(ReadOnlySpan<byte> typeTags, ReadOnlySpan<byte> data, ref int index)
    {
        var values = new object?[calculateArgumentsLength(typeTags)];
        var valueIndex = 0;

        for (var i = 0; i < typeTags.Length; i++)
        {
            var type = typeTags[i];

            if (type == OSCConst.ARRAY_BEGIN)
            {
                var arrayEnd = OSCUtils.FindMatchingArrayEnd(typeTags, i);
                var arr = readArguments(typeTags[(i + 1)..arrayEnd], data, ref index);
                values[valueIndex++] = arr;
                i = arrayEnd;
                continue;
            }

            values[valueIndex++] = type switch
            {
                OSCConst.STRING or OSCConst.ALT_STRING => readString(data, ref index),
                OSCConst.INT => readInt(data, ref index),
                OSCConst.INFINITY => float.PositiveInfinity,
                OSCConst.FLOAT => readFloat(data, ref index),
                OSCConst.TRUE => true,
                OSCConst.FALSE => false,
                OSCConst.BLOB => readBlob(data, ref index),
                OSCConst.LONG => readLong(data, ref index),
                OSCConst.DOUBLE => readDouble(data, ref index),
                OSCConst.CHAR => readChar(data, ref index),
                OSCConst.NIL => null,
                OSCConst.RGBA => readRGBA(data, ref index),
                OSCConst.MIDI => readMidi(data, ref index),
                OSCConst.TIMETAG => readTimeTag(data, ref index),
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char readChar(ReadOnlySpan<byte> data, ref int index)
    {
        var value = readInt(data, ref index);
        return (char)(value & 0xFF);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OSCRGBA readRGBA(ReadOnlySpan<byte> data, ref int index)
    {
        var value = readInt(data, ref index);
        return new OSCRGBA((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OSCMidi readMidi(ReadOnlySpan<byte> data, ref int index)
    {
        var value = readInt(data, ref index);
        return new OSCMidi((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OSCTimeTag readTimeTag(ReadOnlySpan<byte> data, ref int index)
    {
        var value = readULong(data, ref index);
        return new OSCTimeTag(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string readString(ReadOnlySpan<byte> data, ref int index)
    {
        var start = index;
        index = OSCUtils.FindNullTerminator(data, index);

        var stringData = encoding.GetString(data.Slice(start, index - start));
        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based
        return stringData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] readBlob(ReadOnlySpan<byte> data, ref int index)
    {
        var length = readInt(data, ref index);
        var byteArray = data.Slice(index, length).ToArray();
        index += OSCUtils.Align(length);
        return byteArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int readInt(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(data[index..]);
        index += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long readLong(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt64BigEndian(data[index..]);
        index += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong readULong(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadUInt64BigEndian(data[index..]);
        index += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float readFloat(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadSingleBigEndian(data[index..]);
        index += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double readDouble(ReadOnlySpan<byte> data, ref int index)
    {
        var value = BinaryPrimitives.ReadDoubleBigEndian(data[index..]);
        index += 8;
        return value;
    }

    #endregion
}