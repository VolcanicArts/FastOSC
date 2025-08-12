// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable LoopCanBeConvertedToQuery

namespace FastOSC;

public static class OSCEncoder
{
    private static readonly UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: false);

    #region Bundle

    /// <summary>
    /// Encodes an <see cref="OSCBundle"/> into a heap-allocated array.
    /// </summary>
    /// <param name="bundle">The bundle to encode</param>
    /// <returns>A heap-allocated byte array encoded with the contents of <paramref name="bundle"/></returns>
    public static byte[] Encode(OSCBundle bundle)
    {
        var data = new byte[GetEncodedLength(bundle)];
        Encode(bundle, data);
        return data;
    }

    /// <summary>
    /// Encodes an <see cref="OSCBundle"/> into a given destination array.
    /// </summary>
    /// <param name="bundle">The bundle to encode</param>
    /// <param name="dest">The destination array</param>
    /// <remarks>
    /// You can call <see cref="GetEncodedLength(FastOSC.OSCBundle)"/> to rent the exact size for <paramref name="dest"/>
    /// </remarks>
    public static void Encode(OSCBundle bundle, Span<byte> dest)
    {
        var index = 0;
        encodeBundle(dest, ref index, bundle);
    }

    /// <summary>
    /// Calculates the encoded length of the <see cref="OSCBundle"/>
    /// </summary>
    public static int GetEncodedLength(OSCBundle bundle)
    {
        var length = 16; // header + timetag length

        foreach (var packet in bundle.Packets)
        {
            length += packet switch
            {
                OSCBundle nestedBundle => GetEncodedLength(nestedBundle) + 4, // +4 for bundle element length
                OSCMessage message => GetEncodedLength(message) + 4, // +4 for bundle element length
                _ => throw new ArgumentOutOfRangeException(nameof(bundle), bundle, $"Unknown {nameof(IOSCPacket)} within bundle")
            };
        }

        return length;
    }

    private static void encodeBundle(Span<byte> data, ref int index, OSCBundle bundle)
    {
        "#bundle\0"u8.CopyTo(data.Slice(index, 8));
        index += 8;

        writeTimeTag(data, ref index, bundle.TimeTag);

        foreach (var element in bundle.Packets)
        {
            switch (element)
            {
                case OSCBundle subBundle:
                    writeInt(data, ref index, GetEncodedLength(subBundle));
                    encodeBundle(data, ref index, subBundle);
                    break;

                case OSCMessage message:
                    writeInt(data, ref index, GetEncodedLength(message));
                    encodeMessage(data, ref index, message);
                    break;
            }
        }
    }

    #endregion

    #region Message

    /// <summary>
    /// Encodes an <see cref="OSCMessage"/> into a heap-allocated byte array.
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <returns>A heap-allocated byte array encoded with the contents of <paramref name="message"/></returns>
    public static byte[] Encode(OSCMessage message)
    {
        var data = new byte[GetEncodedLength(message)];
        Encode(message, data);
        return data;
    }

    /// <summary>
    /// Encodes an <see cref="OSCMessage"/> into a given destination array.
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <param name="dest">The destination array</param>
    /// <remarks>
    /// You can call <see cref="GetEncodedLength(FastOSC.OSCMessage)"/> to rent the exact size for <paramref name="dest"/>
    /// </remarks>
    public static void Encode(OSCMessage message, Span<byte> dest)
    {
        var index = 0;
        encodeMessage(dest, ref index, message);
    }

    /// <summary>
    /// Calculates the encoded length of the <see cref="OSCMessage"/>
    /// </summary>
    public static int GetEncodedLength(OSCMessage message) => OSCUtils.Align(encoding.GetByteCount(message.Address) + 1) // +1 for null terminator
                                                              + OSCUtils.Align(calculateTypeTagsLength(message.Arguments) + 2) // +2 for comma + null terminator
                                                              + calculateArgumentsLength(message.Arguments);

    private static void encodeMessage(Span<byte> data, ref int index, OSCMessage message)
    {
        writeString(data, ref index, message.Address);
        writeTypeTags(data, ref index, message.Arguments);
        writeArguments(data, ref index, message.Arguments);
    }

    #endregion

    #region Encoding

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
                string str => OSCUtils.Align(encoding.GetByteCount(str) + 1), // +1 for null terminator
                int => 4,
                float.PositiveInfinity => 0,
                float => 4,
                byte[] blob => OSCUtils.Align(blob.Length) + 4, // +4 for encoded length
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

    private static void writeTypeTags(Span<byte> data, ref int index, object?[] arguments)
    {
        data[index++] = OSCConst.COMMA;
        writeTypeTagSymbols(data, ref index, arguments);
        OSCUtils.AlignAndWriteNulls(data, ref index, true);
    }

    private static void writeTypeTagSymbols(Span<byte> data, ref int index, object?[] arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument is object?[] arrayArguments)
            {
                data[index++] = OSCConst.ARRAY_BEGIN;
                writeTypeTagSymbols(data, ref index, arrayArguments);
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

    private static void writeArguments(Span<byte> data, ref int index, object?[] values)
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
                    writeInt(data, ref index, intValue);
                    break;

                case float floatValue:
                    writeFloat(data, ref index, floatValue);
                    break;

                case string stringValue:
                    writeString(data, ref index, stringValue);
                    break;

                case byte[] blobValue:
                    writeBlob(data, ref index, blobValue);
                    break;

                case long longValue:
                    writeLong(data, ref index, longValue);
                    break;

                case double doubleValue:
                    writeDouble(data, ref index, doubleValue);
                    break;

                case char charValue:
                    writeChar(data, ref index, charValue);
                    break;

                case OSCRGBA rgbaValue:
                    writeRGBA(data, ref index, rgbaValue);
                    break;

                case OSCMidi midiValue:
                    writeMidi(data, ref index, midiValue);
                    break;

                case OSCTimeTag timeTagValue:
                    writeTimeTag(data, ref index, timeTagValue);
                    break;

                case object?[] subArrayArguments:
                    writeArguments(data, ref index, subArrayArguments);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{value.GetType()} is an unsupported type");
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeChar(Span<byte> data, ref int index, char v) => writeInt(data, ref index, v & 0xFF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeRGBA(Span<byte> data, ref int index, OSCRGBA v) => writeInt(data, ref index, v.R << 24 | v.G << 16 | v.B << 8 | v.A);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeMidi(Span<byte> data, ref int index, OSCMidi v) => writeInt(data, ref index, v.PortID << 24 | v.Status << 16 | v.Data1 << 8 | v.Data2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeTimeTag(Span<byte> data, ref int index, OSCTimeTag v) => writeUlong(data, ref index, v.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeString(Span<byte> data, ref int index, string value)
    {
        var bytesWritten = encoding.GetBytes(value, data[index..]);
        index += bytesWritten;
        OSCUtils.AlignAndWriteNulls(data, ref index, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeBlob(Span<byte> data, ref int index, byte[] value)
    {
        var length = value.Length;
        writeInt(data, ref index, length);

        value.CopyTo(data[index..]);
        index += length;

        OSCUtils.AlignAndWriteNulls(data, ref index, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeInt(Span<byte> data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data[index..], value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeLong(Span<byte> data, ref int index, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(data[index..], value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeUlong(Span<byte> data, ref int index, ulong value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(data[index..], value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeFloat(Span<byte> data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data[index..], value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeDouble(Span<byte> data, ref int index, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(data[index..], value);
        index += 8;
    }

    #endregion
}