// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable LoopCanBeConvertedToQuery

namespace FastOSC;

public static class OSCEncoder
{
    private static readonly Encoding encoding = Encoding.UTF8;

    #region Bundle

    public static byte[] Encode(OSCBundle bundle)
    {
        var index = 0;
        var data = new byte[calculateBundleLength(bundle)];

        encodeBundle(data, ref index, bundle);
        return data;
    }

    private static void encodeBundle(Span<byte> data, ref int index, OSCBundle bundle)
    {
        "#bundle\0"u8.CopyTo(data.Slice(index, 8));
        index += 8;

        encodeTimeTag(data, ref index, bundle.TimeTag);

        foreach (var element in bundle.Packets)
        {
            switch (element)
            {
                case OSCBundle subBundle:
                    encodeInt(data, ref index, calculateBundleLength(subBundle));
                    encodeBundle(data, ref index, subBundle);
                    break;

                case OSCMessage message:
                    encodeInt(data, ref index, calculateMessageLength(message));
                    encodeMessage(data, ref index, message);
                    break;
            }
        }
    }

    private static int calculateBundleLength(OSCBundle bundle)
    {
        var length = 16; // header + timetag length

        foreach (var packet in bundle.Packets)
        {
            length += packet switch
            {
                OSCBundle nestedBundle => calculateBundleLength(nestedBundle) + 4, // bundle element length
                OSCMessage message => calculateMessageLength(message) + 4, // bundle element length
                _ => throw new ArgumentOutOfRangeException(nameof(bundle), bundle, $"Unknown {nameof(IOSCPacket)} within bundle")
            };
        }

        return length;
    }

    #endregion

    #region Message

    /// <summary>
    /// Encodes an <see cref="OSCMessage"/> into a given destination array.
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <param name="dest">The destination array</param>
    /// <remarks>
    /// This is useful if you want to use your own array pool to have the encoding allocate no memory on the heap.
    /// You can call <see cref="GetEncodedSize"/> to rent the exact size for <paramref name="dest"/>
    /// </remarks>
    public static void Encode(OSCMessage message, Span<byte> dest)
    {
        var index = 0;
        encodeMessage(dest, ref index, message);
    }

    /// <summary>
    /// Encodes an <see cref="OSCMessage"/> into a created array. The size of the created array fits the encoded message
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <returns>A heap-allocated byte array encoded with the contents of <paramref name="message"/></returns>
    public static byte[] Encode(OSCMessage message)
    {
        var index = 0;
        var data = new byte[calculateMessageLength(message)];

        encodeMessage(data, ref index, message);
        return data;
    }

    /// <summary>
    /// Calculates the encoded size of the <see cref="OSCMessage"/>
    /// </summary>
    public static int GetEncodedSize(OSCMessage message) => calculateMessageLength(message);

    private static int calculateMessageLength(OSCMessage message) => OSCUtils.Align(encoding.GetByteCount(message.Address) + 1) // +1 for null terminator
                                                                     + OSCUtils.Align(calculateTypeTagsLength(message.Arguments) + 2) // +2 for comma + null terminator
                                                                     + calculateArgumentsLength(message.Arguments);

    private static void encodeMessage(Span<byte> data, ref int index, OSCMessage message)
    {
        encodeString(data, ref index, message.Address);
        insertTypeTags(data, ref index, message.Arguments);
        insertArguments(data, ref index, message.Arguments);
    }

    private static int calculateTypeTagsLength(object?[] arguments)
    {
        var length = 0;

        foreach (var argument in arguments)
        {
            if (argument is object?[] internalArrayValue)
                length += calculateTypeTagsLength(internalArrayValue) + 2; // '[' + ']' length
            else
                length += 1;
        }

        return length;
    }

    private static int calculateArgumentsLength(object?[] arguments)
    {
        var length = 0;

        foreach (var value in arguments)
        {
            length += value switch
            {
                string valueStr => OSCUtils.Align(encoding.GetByteCount(valueStr) + 1), // +1 for null terminator
                int => 4,
                float.PositiveInfinity => 0,
                float => 4,
                byte[] valueByteArray => OSCUtils.Align(valueByteArray.Length) + 4, // +4 for encoded length
                long => 8,
                double => 8,
                OSCTimeTag => 8,
                char => 4,
                OSCRGBA => 4,
                OSCMidi => 4,
                null => 0,
                bool => 0,
                object?[] subArrayArguments => calculateArgumentsLength(subArrayArguments),
                _ => throw new ArgumentOutOfRangeException($"{value.GetType()} is an unsupported type")
            };
        }

        return length;
    }

    private static void insertTypeTags(Span<byte> data, ref int index, object?[] arguments)
    {
        data[index++] = OSCConst.COMMA;
        insertTypeTagSymbols(data, ref index, arguments);
        OSCUtils.AlignAndWriteNulls(data, ref index, true);
    }

    private static void insertTypeTagSymbols(Span<byte> data, ref int index, object?[] arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument is object?[] arrayArguments)
            {
                data[index++] = OSCConst.ARRAY_BEGIN;
                insertTypeTagSymbols(data, ref index, arrayArguments);
                data[index++] = OSCConst.ARRAY_END;
                continue;
            }

            data[index++] = argument switch
            {
                string => OSCConst.STRING,
                int => OSCConst.INT,
                float.PositiveInfinity => OSCConst.INFINITY,
                float => OSCConst.FLOAT,
                true => OSCConst.TRUE,
                false => OSCConst.FALSE,
                byte[] => OSCConst.BLOB,
                long => OSCConst.LONG,
                double => OSCConst.DOUBLE,
                char => OSCConst.CHAR,
                null => OSCConst.NIL,
                OSCRGBA => OSCConst.RGBA,
                OSCMidi => OSCConst.MIDI,
                OSCTimeTag => OSCConst.TIMETAG,
                _ => throw new ArgumentOutOfRangeException($"{argument.GetType()} is an unsupported type")
            };
        }
    }

    private static void insertArguments(Span<byte> data, ref int index, object?[] values)
    {
        foreach (var value in values)
        {
            switch (value)
            {
                case true:
                case false:
                case null:
                case float.PositiveInfinity:
                    break;

                case int intValue:
                    encodeInt(data, ref index, intValue);
                    break;

                case float floatValue:
                    encodeFloat(data, ref index, floatValue);
                    break;

                case string stringValue:
                    encodeString(data, ref index, stringValue);
                    break;

                case byte[] byteArrayValue:
                    encodeByteArray(data, ref index, byteArrayValue);
                    break;

                case long longValue:
                    encodeLong(data, ref index, longValue);
                    break;

                case double doubleValue:
                    encodeDouble(data, ref index, doubleValue);
                    break;

                case char charValue:
                    encodeChar(data, ref index, charValue);
                    break;

                case OSCRGBA rgbaValue:
                    encodeRGBA(data, ref index, rgbaValue);
                    break;

                case OSCMidi midiValue:
                    encodeMidi(data, ref index, midiValue);
                    break;

                case OSCTimeTag timeTagValue:
                    encodeTimeTag(data, ref index, timeTagValue);
                    break;

                case object?[] subArrayArguments:
                    insertArguments(data, ref index, subArrayArguments);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeInt(Span<byte> data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data.Slice(index, 4), value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeFloat(Span<byte> data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.Slice(index, 4), value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeString(Span<byte> data, ref int index, string value)
    {
        var bytesWritten = encoding.GetBytes(value, data[index..]);
        index += bytesWritten;
        OSCUtils.AlignAndWriteNulls(data, ref index, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeByteArray(Span<byte> data, ref int index, byte[] value)
    {
        var length = value.Length;
        encodeInt(data, ref index, length);
        value.AsSpan().CopyTo(data.Slice(index, length));
        index += length;
        OSCUtils.AlignAndWriteNulls(data, ref index, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeLong(Span<byte> data, ref int index, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(data.Slice(index, 8), value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeDouble(Span<byte> data, ref int index, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(data.Slice(index, 8), value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeChar(Span<byte> data, ref int index, char value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(data.Slice(index, 4), (uint)(value & 0xFF));
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeRGBA(Span<byte> data, ref int index, OSCRGBA value)
    {
        data[index++] = value.R;
        data[index++] = value.G;
        data[index++] = value.B;
        data[index++] = value.A;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeMidi(Span<byte> data, ref int index, OSCMidi midi)
    {
        data[index++] = midi.PortID;
        data[index++] = midi.Status;
        data[index++] = midi.Data1;
        data[index++] = midi.Data2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void encodeTimeTag(Span<byte> data, ref int index, OSCTimeTag timeTag)
    {
        BinaryPrimitives.WriteUInt64BigEndian(data.Slice(index, 8), (ulong)timeTag);
        index += 8;
    }

    #endregion
}