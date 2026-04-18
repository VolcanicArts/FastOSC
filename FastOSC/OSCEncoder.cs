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
    /// Encodes an <see cref="OSCBundle"/> into a heap-allocated byte array.
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
    /// Encodes an <see cref="OSCBundle"/> into a given destination <see cref="Span{byte}"/>.
    /// </summary>
    /// <param name="bundle">The bundle to encode</param>
    /// <param name="dest">The destination <see cref="Span{byte}"/></param>
    /// <remarks>
    /// You can call <see cref="GetEncodedLength(FastOSC.OSCBundle)"/> to rent the exact size for <paramref name="dest"/>
    /// </remarks>
    public static void Encode(OSCBundle bundle, Span<byte> dest)
    {
        var index = 0;
        encodeBundle(dest, ref index, bundle);
    }

    /// <summary>
    /// Calculates the encoded length of an <see cref="OSCBundle"/>
    /// </summary>
    /// <param name="bundle">The bundle to calculate the encoded length for</param>
    /// <returns>The encoded length of the <paramref name="bundle"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if an unknown <see cref="IOSCPacket"/> is inside the provided <paramref name="bundle"/>, or any nested bundles</exception>
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
    /// Encodes an <see cref="OSCMessage"/> into a given destination <see cref="Span{byte}"/>.
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <param name="dest">The destination <see cref="Span{byte}"/></param>
    /// <remarks>
    /// You can call <see cref="GetEncodedLength(FastOSC.OSCMessage)"/> to rent the exact size for <paramref name="dest"/>
    /// </remarks>
    public static void Encode(OSCMessage message, Span<byte> dest)
    {
        var index = 0;
        encodeMessage(dest, ref index, message);
    }

    /// <summary>
    /// Calculates the encoded length of an <see cref="OSCMessage"/>
    /// </summary>
    /// <param name="message">The message to calculate the encoded length for</param>
    /// <returns>The encoded length of the <paramref name="message"/></returns>
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

    private static void calculateLengths(ReadOnlySpan<object> arguments, out int typeTagsLength, out int argumentsLength)
    {
        typeTagsLength = 0;
        argumentsLength = 0;

        foreach (var argument in arguments)
        {
            switch (argument)
            {
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

                case float:
                case int:
                case char:
                case OSCRGBA:
                case OSCMIDI:
                    typeTagsLength += 1;
                    argumentsLength += 4;
                    break;

                case bool:
                case OSCNil:
                case OSCInfinitum:
                    typeTagsLength += 1;
                    break;

                case object[] sub:
                    calculateLengths(sub, out var subTypeTagsLength, out var subArgumentsLength);
                    typeTagsLength += subTypeTagsLength + 2;
                    argumentsLength += subArgumentsLength;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{argument.GetType()} is an unsupported type");
            }
        }
    }

    private static void writeTypeTags(Span<byte> data, ref int index, ReadOnlySpan<object> arguments)
    {
        data[index++] = OSCChar.COMMA;
        writeTypeTagSymbols(data, ref index, arguments);
        OSCUtils.AlignAndWriteNullsWithTerminator(data, ref index);
    }

    private static void writeTypeTagSymbols(Span<byte> data, ref int index, ReadOnlySpan<object> arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument is object[] arrayArgument)
            {
                data[index++] = OSCChar.ARRAY_BEGIN;
                writeTypeTagSymbols(data, ref index, arrayArgument);
                data[index++] = OSCChar.ARRAY_END;
                continue;
            }

            data[index++] = argument switch
            {
                string => OSCChar.STRING,
                int => OSCChar.INT,
                float => OSCChar.FLOAT,
                true => OSCChar.TRUE,
                false => OSCChar.FALSE,
                byte[] => OSCChar.BLOB,
                long => OSCChar.LONG,
                double => OSCChar.DOUBLE,
                char => OSCChar.CHAR,
                OSCNil => OSCChar.NIL,
                OSCInfinitum => OSCChar.INFINITUM,
                OSCRGBA => OSCChar.RGBA,
                OSCMIDI => OSCChar.MIDI,
                OSCTimeTag => OSCChar.TIMETAG,
                _ => throw new ArgumentOutOfRangeException($"{argument.GetType()} is an unsupported type")
            };
        }
    }

    private static void writeArguments(Span<byte> data, ref int index, ReadOnlySpan<object> values)
    {
        foreach (var value in values)
        {
            switch (value)
            {
                case true:
                case false:
                case OSCNil:
                case OSCInfinitum:
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

                case OSCMIDI midiValue:
                    writeMidi(data, ref index, midiValue);
                    break;

                case OSCTimeTag timeTagValue:
                    writeTimeTag(data, ref index, timeTagValue);
                    break;

                case object[] subArrayArguments:
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
    private static void writeMidi(Span<byte> data, ref int index, OSCMIDI v) => writeIntLE(data, ref index, Unsafe.BitCast<OSCMIDI, int>(v));

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
    private static void writeBlob(Span<byte> data, ref int index, ReadOnlySpan<byte> value)
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