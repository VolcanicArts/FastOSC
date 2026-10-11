// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeMethodOrOperatorBody

namespace FastOSC;

public static class OSCDecoder
{
    private static readonly UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly ulong bundle_header = MemoryMarshal.Read<ulong>("#bundle\0"u8);

    /// <summary>
    /// Decodes a <see cref="ReadOnlySpan{byte}"/> into an <see cref="IOSCPacket"/>.
    /// </summary>
    /// <param name="data">The data to decode</param>
    /// <returns>The decoded <see cref="IOSCPacket"/></returns>
    public static IOSCPacket Decode(ReadOnlySpan<byte> data)
    {
        var index = 0;
        return decode(data, ref index);
    }

    private static IOSCPacket decode(ReadOnlySpan<byte> data, ref int index)
    {
        var start = index;
        return readULongLE(data, ref start) == bundle_header ? decodeBundle(data, ref index) : decodeMessage(data, ref index);
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
            var packetLength = readIntBE(data, ref packetIndex);
            packetIndex += packetLength;
            packetCount++;
        }

        var packets = new IOSCPacket[packetCount];

        for (var i = 0; i < packetCount; i++)
        {
            var packetLength = readIntBE(data, ref index);
            var packet = Decode(data.Slice(index, packetLength));
            index += packetLength;
            packets[i] = packet ?? throw new Exception("A packet inside a bundle was unable to be decoded");
        }

        return new OSCBundle(timeTag, packets);
    }

    #endregion

    #region Message

    private static OSCMessage decodeMessage(ReadOnlySpan<byte> data, ref int index)
    {
        var address = readAddress(data, ref index);
        if (string.IsNullOrEmpty(address)) throw new Exception("No address was found");

        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based
        var addressLength = index;

        var typeTags = readTypeTags(data, ref index);
        if (typeTags.IsEmpty) throw new Exception("No type tags were found");

        index = OSCUtils.Align(index + 1); // +1 to adjust 0th-based to 1st-based
        var typeTagLength = index - addressLength;

        var arguments = readArguments(typeTags, data, ref index);
        var argumentsLength = index - typeTagLength - addressLength;

        return new OSCMessage(address, arguments, typeTagLength, argumentsLength);
    }

    private static string? readAddress(ReadOnlySpan<byte> data, ref int index)
    {
        if (data[index] != OSCChar.SLASH) return null;

        var start = index;
        index = OSCUtils.FindNullTerminator(data, index);
        return encoding.GetString(data.Slice(start, index - start));
    }

    private static ReadOnlySpan<byte> readTypeTags(ReadOnlySpan<byte> data, ref int index)
    {
        if (data[index] != OSCChar.COMMA) return ReadOnlySpan<byte>.Empty;

        var start = index;
        index = OSCUtils.FindNullTerminator(data, index);
        return data.Slice(start + 1, index - (start + 1));
    }

    private static object[] readArguments(ReadOnlySpan<byte> typeTags, ReadOnlySpan<byte> data, ref int index)
    {
        var values = new object[calculateArgumentsLength(typeTags)];
        var valueIndex = 0;

        for (var i = 0; i < typeTags.Length; i++)
        {
            var type = typeTags[i];

            if (type == OSCChar.ARRAY_BEGIN)
            {
                var arrayEnd = OSCUtils.FindMatchingArrayEnd(typeTags, i);
                var arr = readArguments(typeTags[(i + 1)..arrayEnd], data, ref index);
                values[valueIndex++] = arr;
                i = arrayEnd;
                continue;
            }

            values[valueIndex++] = type switch
            {
                OSCChar.STRING or OSCChar.ALT_STRING => readString(data, ref index),
                OSCChar.INT => readIntBE(data, ref index),
                OSCChar.INFINITUM => OSC.INFINITUM,
                OSCChar.FLOAT => readFloat(data, ref index),
                OSCChar.TRUE => true,
                OSCChar.FALSE => false,
                OSCChar.BLOB => readBlob(data, ref index),
                OSCChar.LONG => readLong(data, ref index),
                OSCChar.DOUBLE => readDouble(data, ref index),
                OSCChar.CHAR => readChar(data, ref index),
                OSCChar.NIL => OSC.NIL,
                OSCChar.RGBA => readRGBA(data, ref index),
                OSCChar.MIDI => readMIDI(data, ref index),
                OSCChar.TIMETAG => readTimeTag(data, ref index),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unable to decode unknown argument type")
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
            if (typeTags[i] == OSCChar.ARRAY_BEGIN)
            {
                length++;
                var nestedLevel = 1;

                i++;

                while (i < typeTags.Length && nestedLevel > 0)
                {
                    switch (typeTags[i])
                    {
                        case OSCChar.ARRAY_BEGIN:
                            nestedLevel++;
                            break;

                        case OSCChar.ARRAY_END:
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
    private static char readChar(ReadOnlySpan<byte> data, ref int index) => (char)readIntLE(data, ref index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OSCRGBA readRGBA(ReadOnlySpan<byte> data, ref int index) => Unsafe.BitCast<int, OSCRGBA>(readIntLE(data, ref index));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OSCMIDI readMIDI(ReadOnlySpan<byte> data, ref int index) => Unsafe.BitCast<int, OSCMIDI>(readIntLE(data, ref index));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OSCTimeTag readTimeTag(ReadOnlySpan<byte> data, ref int index)
    {
        var value = readULongBE(data, ref index);
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
        var length = readIntBE(data, ref index);
        var byteArray = data.Slice(index, length).ToArray();
        index += OSCUtils.Align(length);
        return byteArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int readIntBE(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(data[index..]));
        index += 4;

        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int readIntLE(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(data[index..]));
        index += 4;

        return !BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long readLong(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(data[index..]));
        index += 8;

        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong readULongLE(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(data[index..]));
        index += 8;

        return !BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong readULongBE(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<ulong>(ref MemoryMarshal.GetReference(data[index..]));
        index += 8;

        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float readFloat(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(data[index..]));
        index += 4;

        value = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        return BitConverter.Int32BitsToSingle(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double readDouble(ReadOnlySpan<byte> data, ref int index)
    {
        var value = Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(data[index..]));
        index += 8;

        value = BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        return BitConverter.Int64BitsToDouble(value);
    }

    #endregion
}