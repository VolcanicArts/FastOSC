// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeMethodOrOperatorBody
// ReSharper disable LoopCanBeConvertedToQuery

namespace FastOSC;

public static class OSCEncoder
{
    private static readonly Encoding encoding = Encoding.UTF8;
    private static readonly ConcurrentDictionary<string, byte[]> str_cache = new();

    private static ReadOnlySpan<byte> stringToBytes(string str)
    {
        if (str_cache.TryGetValue(str, out var foundBytes)) return foundBytes;

        var bytes = encoding.GetBytes(str);
        str_cache.TryAdd(str, bytes);
        return bytes;
    }

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
                    var addressBytes = stringToBytes(message.Address);
                    encodeInt(data, ref index, calculateMessageLength(message, addressBytes));
                    encodeMessage(data, ref index, message, addressBytes);
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
                OSCMessage message => calculateMessageLength(message, stringToBytes(message.Address)) + 4, // bundle element length
                _ => throw new ArgumentOutOfRangeException()
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
    /// Ensure that <paramref name="dest"/> is large enough to handle your largest message
    /// </remarks>
    public static void Encode(OSCMessage message, Span<byte> dest)
    {
        var index = 0;
        encodeMessage(dest, ref index, message, stringToBytes(message.Address));
    }

    /// <summary>
    /// Encodes an <see cref="OSCMessage"/> into a created array. The size of the created array fits the encoded message
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <returns>A heap-allocated byte array encoded with the contents of <paramref name="message"/></returns>
    /// <remarks>Caching is used where possible to reduce allocations, so the total allocations for this method of encoding is however large the returned array is</remarks>
    public static byte[] Encode(OSCMessage message)
    {
        var index = 0;
        var addressBytes = stringToBytes(message.Address);

        var data = new byte[calculateMessageLength(message, addressBytes)];
        encodeMessage(data, ref index, message, addressBytes);
        return data;
    }

    private static int calculateMessageLength(OSCMessage message, ReadOnlySpan<byte> addressBytes)
    {
        return OSCUtils.Align(addressBytes.Length)
               + OSCUtils.Align(calculateTypeTagsLength(message.Arguments))
               + calculateArgumentsLength(message.Arguments);
    }

    private static void encodeMessage(Span<byte> data, ref int index, OSCMessage message, ReadOnlySpan<byte> addressBytes)
    {
        var addressLength = OSCUtils.Align(addressBytes.Length);
        addressBytes.CopyTo(data.Slice(index, addressLength));
        index += addressLength;

        insertTypeTags(data, ref index, message.Arguments);
        insertArguments(data, ref index, message.Arguments);
    }

    private static int calculateTypeTagsLength(object?[] arguments)
    {
        var length = 1; // comma

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
                string valueStr => OSCUtils.Align(encoding.GetByteCount(valueStr)),
                int => 4,
                float.PositiveInfinity => 0,
                float => 4,
                byte[] valueByteArray => OSCUtils.Align(valueByteArray.Length, false) + 4,
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
        index = OSCUtils.Align(index);
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

    private static void encodeInt(Span<byte> data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data.Slice(index, 4), value);
        index += 4;
    }

    private static void encodeFloat(Span<byte> data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.Slice(index, 4), value);
        index += 4;
    }

    private static void encodeString(Span<byte> data, ref int index, string value)
    {
        var bytes = stringToBytes(value);
        var bytesLength = OSCUtils.Align(bytes.Length);
        bytes.CopyTo(data.Slice(index, bytesLength));
        index += bytesLength;
    }

    private static void encodeByteArray(Span<byte> data, ref int index, byte[] value)
    {
        var length = value.Length;
        encodeInt(data, ref index, length);
        value.CopyTo(data.Slice(index, length));

        index += OSCUtils.Align(length);
    }

    private static void encodeLong(Span<byte> data, ref int index, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(data.Slice(index, 8), value);
        index += 8;
    }

    private static void encodeDouble(Span<byte> data, ref int index, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(data.Slice(index, 8), value);
        index += 8;
    }

    private static void encodeChar(Span<byte> data, ref int index, char value)
    {
        var charBytes = encoding.GetBytes([value]);

        for (var i = 0; i < charBytes.Length; i++)
        {
            data[index + 3 - i] = charBytes[charBytes.Length - 1 - i];
        }

        index += 4;
    }

    private static void encodeRGBA(Span<byte> data, ref int index, OSCRGBA value)
    {
        data[index++] = value.R;
        data[index++] = value.G;
        data[index++] = value.B;
        data[index++] = value.A;
    }

    private static void encodeMidi(Span<byte> data, ref int index, OSCMidi midi)
    {
        data[index++] = midi.PortID;
        data[index++] = midi.Status;
        data[index++] = midi.Data1;
        data[index++] = midi.Data2;
    }

    private static void encodeTimeTag(Span<byte> data, ref int index, OSCTimeTag timeTag)
    {
        BinaryPrimitives.WriteUInt64BigEndian(data.Slice(index, 8), (ulong)timeTag);
        index += 8;
    }

    #endregion
}