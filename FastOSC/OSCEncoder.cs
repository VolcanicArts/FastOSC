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
            var lengthIndex = index;
            index += 4;
            var elementIndex = index;

            switch (element)
            {
                case OSCBundle subBundle:
                    encodeBundle(data, ref index, subBundle);
                    break;

                case OSCMessage message:
                    encodeMessage(data, ref index, message);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(bundle), bundle, $"Unknown {nameof(IOSCPacket)} within bundle");
            }

            writeIntBE(data, ref lengthIndex, index - elementIndex);
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
    public static int GetEncodedLength(OSCMessage message)
    {
        var addressLength = OSCUtils.Align(encoding.GetByteCount(message.Address) + 1); // +1 for null terminator
        calculateLengths(message.Arguments, out var typeTagsLength, out var argumentsLength);
        return addressLength + OSCUtils.Align(typeTagsLength + 2) + argumentsLength; // +2 for comma + null terminator
    }

    private static void encodeMessage(Span<byte> data, ref int index, OSCMessage message)
    {
        writeString(data, ref index, message.Address);
        writeTypeTags(data, ref index, message.Arguments);
        writeArguments(data, ref index, message.Arguments);
    }

    #endregion

    #region Encoding

    private static void calculateLengths(ReadOnlySpan<object?> arguments, out int typeTagsLength, out int argumentsLength)
    {
        typeTagsLength = 0;
        argumentsLength = 0;

        foreach (var argument in arguments)
        {
            switch (argument)
            {
                case float f:
                    typeTagsLength += 1;
                    argumentsLength += float.IsPositiveInfinity(f) ? 0 : 4;
                    break;

                case string str:
                    typeTagsLength += 1;
                    argumentsLength += OSCUtils.Align(encoding.GetByteCount(str) + 1);
                    break;

                case byte[] blob:
                    typeTagsLength += 1;
                    argumentsLength += OSCUtils.Align(blob.Length) + 4;
                    break;

                case long:
                case double:
                case OSCTimeTag:
                    typeTagsLength += 1;
                    argumentsLength += 8;
                    break;

                case int:
                case char:
                case OSCRGBA:
                case OSCMidi:
                    typeTagsLength += 1;
                    argumentsLength += 4;
                    break;

                case null:
                case bool:
                    typeTagsLength += 1;
                    break;

                case object?[] sub:
                    calculateLengths(sub, out var subTypeTagsLength, out var subArgumentsLength);
                    typeTagsLength += subTypeTagsLength + 2;
                    argumentsLength += subArgumentsLength;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{argument.GetType()} is an unsupported type");
            }
        }
    }

    private static void writeTypeTags(Span<byte> data, ref int index, ReadOnlySpan<object?> arguments)
    {
        data[index++] = OSCConst.COMMA;
        writeTypeTagSymbols(data, ref index, arguments);
        OSCUtils.AlignAndWriteNullsWithTerminator(data, ref index);
    }

    private static void writeTypeTagSymbols(Span<byte> data, ref int index, ReadOnlySpan<object?> arguments)
    {
        foreach (var argument in arguments)
        {
            switch (argument)
            {
                case string:
                    data[index++] = OSCConst.STRING;
                    break;

                case int:
                    data[index++] = OSCConst.INT;
                    break;

                case float floatArgument:
                    data[index++] = float.IsPositiveInfinity(floatArgument) ? OSCConst.INFINITY : OSCConst.FLOAT;
                    break;

                case true:
                    data[index++] = OSCConst.TRUE;
                    break;

                case false:
                    data[index++] = OSCConst.FALSE;
                    break;

                case byte[]:
                    data[index++] = OSCConst.BLOB;
                    break;

                case long:
                    data[index++] = OSCConst.LONG;
                    break;

                case double:
                    data[index++] = OSCConst.DOUBLE;
                    break;

                case char:
                    data[index++] = OSCConst.CHAR;
                    break;

                case null:
                    data[index++] = OSCConst.NIL;
                    break;

                case OSCRGBA:
                    data[index++] = OSCConst.RGBA;
                    break;

                case OSCMidi:
                    data[index++] = OSCConst.MIDI;
                    break;

                case OSCTimeTag:
                    data[index++] = OSCConst.TIMETAG;
                    break;

                case object?[] arrayArgument:
                    data[index++] = OSCConst.ARRAY_BEGIN;
                    writeTypeTagSymbols(data, ref index, arrayArgument);
                    data[index++] = OSCConst.ARRAY_END;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{argument.GetType()} is an unsupported type");
            }
        }
    }

    private static void writeArguments(Span<byte> data, ref int index, ReadOnlySpan<object?> values)
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
                    writeIntBE(data, ref index, intValue);
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
    private static void writeChar(Span<byte> data, ref int index, char v) => writeIntBE(data, ref index, v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeRGBA(Span<byte> data, ref int index, OSCRGBA v) => writeIntLE(data, ref index, Unsafe.BitCast<OSCRGBA, int>(v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeMidi(Span<byte> data, ref int index, OSCMidi v) => writeIntLE(data, ref index, Unsafe.BitCast<OSCMidi, int>(v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeTimeTag(Span<byte> data, ref int index, OSCTimeTag v) => writeUlong(data, ref index, v.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeString(Span<byte> data, ref int index, string value)
    {
        var bytesWritten = encoding.GetBytes(value, data[index..]);
        index += bytesWritten;
        OSCUtils.AlignAndWriteNullsWithTerminator(data, ref index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeBlob(Span<byte> data, ref int index, byte[] value)
    {
        var length = value.Length;
        writeIntBE(data, ref index, length);

        value.CopyTo(data[index..]);
        index += length;

        OSCUtils.AlignAndWriteNulls(data, ref index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeIntBE(Span<byte> data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data[index..], value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeIntLE(Span<byte> data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(data[index..], value);
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