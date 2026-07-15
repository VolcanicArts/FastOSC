// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable LoopCanBeConvertedToQuery

namespace FastOSC;

public static class OSCEncoder
{
    private static readonly UTF8Encoding encoding = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly ulong bundle_header = MemoryMarshal.Read<ulong>("#bundle\0"u8);

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
    /// <returns>The number of bytes written</returns>
    public static int Encode(OSCBundle bundle, Span<byte> dest)
    {
        var index = 0;
        encodeBundle(dest, ref index, bundle);
        return index;
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
                OSCMessage message => GetEncodedLength(message) + 4, // +4 for bundle element length
                OSCBundle nestedBundle => GetEncodedLength(nestedBundle) + 4, // +4 for bundle element length
                _ => throw new ArgumentOutOfRangeException(nameof(bundle), bundle, $"Unknown {nameof(IOSCPacket)} within bundle")
            };
        }

        return length;
    }

    private static void encodeBundle(Span<byte> data, ref int index, OSCBundle bundle)
    {
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index), bundle_header);
        index += 8;

        writeTimeTag(data, ref index, bundle.TimeTag);

        foreach (var element in bundle.Packets)
        {
            var lengthIndex = index;
            index += 4;
            var elementIndex = index;

            switch (element)
            {
                case OSCMessage message:
                    encodeMessage(data, ref index, message);
                    break;

                case OSCBundle subBundle:
                    encodeBundle(data, ref index, subBundle);
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
    /// Encodes an <see cref="OSCMessage"/> into <paramref name="dest"/>.
    /// </summary>
    /// <param name="message">The message to encode</param>
    /// <param name="dest">The destination <see cref="Span{byte}"/></param>
    /// <returns>The number of bytes written</returns>
    public static int Encode(OSCMessage message, Span<byte> dest)
    {
        var index = 0;
        encodeMessage(dest, ref index, message);
        return index;
    }

    /// <summary>
    /// Calculates the encoded length of an <see cref="OSCMessage"/>
    /// </summary>
    /// <param name="message">The message to calculate the encoded length for</param>
    /// <returns>The encoded length of the <paramref name="message"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetEncodedLength(OSCMessage message)
        => OSCUtils.Align(encoding.GetByteCount(message.Address) + 1) + message.TypeTagsLength + message.ArgumentsLength;

    private static void encodeMessage(Span<byte> data, ref int index, OSCMessage message)
    {
        writeString(data, ref index, message.Address);

        var tagBlockStart = index;
        var tagIndex = tagBlockStart;
        data[tagIndex++] = OSCChar.COMMA;

        var argIndex = tagBlockStart + message.TypeTagsLength;
        writeTagsAndArguments(data, ref tagIndex, ref argIndex, message.Arguments);

        OSCUtils.AlignAndWriteNullsWithTerminator(data, ref tagIndex);
        index = argIndex;
    }

    #endregion

    #region Encoding

    private static void writeTagsAndArguments(Span<byte> data, ref int tagIndex, ref int argIndex, ReadOnlySpan<object> arguments)
    {
        foreach (var argument in arguments)
        {
            switch (argument)
            {
                case int intValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.INT;
                    writeIntBE(data, ref argIndex, intValue);
                    break;

                case float floatValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.FLOAT;
                    writeFloat(data, ref argIndex, floatValue);
                    break;

                case string stringValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.STRING;
                    writeString(data, ref argIndex, stringValue);
                    break;

                case true:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.TRUE;
                    break;

                case false:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.FALSE;
                    break;

                case byte[] blobValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.BLOB;
                    writeBlob(data, ref argIndex, blobValue);
                    break;

                case long longValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.LONG;
                    writeLong(data, ref argIndex, longValue);
                    break;

                case double doubleValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.DOUBLE;
                    writeDouble(data, ref argIndex, doubleValue);
                    break;

                case char charValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.CHAR;
                    writeChar(data, ref argIndex, charValue);
                    break;

                case OSCRGBA rgbaValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.RGBA;
                    writeRGBA(data, ref argIndex, rgbaValue);
                    break;

                case OSCMIDI midiValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.MIDI;
                    writeMidi(data, ref argIndex, midiValue);
                    break;

                case OSCTimeTag timeTagValue:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.TIMETAG;
                    writeTimeTag(data, ref argIndex, timeTagValue);
                    break;

                case OSCNil:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.NIL;
                    break;

                case OSCInfinitum:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.INFINITUM;
                    break;

                case object[] subArray:
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.ARRAY_BEGIN;
                    writeTagsAndArguments(data, ref tagIndex, ref argIndex, subArray);
                    Unsafe.Add(ref MemoryMarshal.GetReference(data), tagIndex++) = OSCChar.ARRAY_END;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{argument?.GetType()} is an unsupported type");
            }
        }
    }

    internal static void CalculateLengths(ReadOnlySpan<object> arguments, ref int typeTagsLength, ref int argumentsLength)
    {
        foreach (var argument in arguments)
        {
            calculateLength(argument, ref typeTagsLength, ref argumentsLength);
        }
    }

    private static void calculateLength(object argument, ref int typeTagLength, ref int argumentLength)
    {
        switch (argument)
        {
            case string str:
                typeTagLength += 1;
                argumentLength += OSCUtils.Align(encoding.GetByteCount(str) + 1); // +1 for null terminator
                break;

            case byte[] blob:
                typeTagLength += 1;
                argumentLength += OSCUtils.Align(blob.Length) + 4; // +4 for length
                break;

            case long:
            case double:
            case OSCTimeTag:
                typeTagLength += 1;
                argumentLength += 8;
                break;

            case float:
            case int:
            case char:
            case OSCRGBA:
            case OSCMIDI:
                typeTagLength += 1;
                argumentLength += 4;
                break;

            case bool:
            case OSCNil:
            case OSCInfinitum:
                typeTagLength += 1;
                break;

            case object[] sub:
                CalculateLengths(sub, ref typeTagLength, ref argumentLength);
                typeTagLength += 2; // +2 for []
                break;

            default:
                throw new ArgumentOutOfRangeException($"{argument?.GetType()} is an unsupported type");
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
        Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index), ref MemoryMarshal.GetReference(value), (uint)length);
        index += length;
        OSCUtils.AlignAndWriteNulls(data, ref index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeIntBE(Span<byte> data, ref int index, int value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index), value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeIntLE(Span<byte> data, ref int index, int value)
    {
        if (!BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index), value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeLong(Span<byte> data, ref int index, long value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index), value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeUlong(Span<byte> data, ref int index, ulong value)
    {
        if (BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index), value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeFloat(Span<byte> data, ref int index, float value)
    {
        ref byte dest = ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index);

        if (BitConverter.IsLittleEndian)
            Unsafe.WriteUnaligned(ref dest, BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value)));
        else
            Unsafe.WriteUnaligned(ref dest, value);

        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void writeDouble(Span<byte> data, ref int index, double value)
    {
        ref byte dest = ref Unsafe.Add(ref MemoryMarshal.GetReference(data), index);

        if (BitConverter.IsLittleEndian)
            Unsafe.WriteUnaligned(ref dest, BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value)));
        else
            Unsafe.WriteUnaligned(ref dest, value);

        index += 8;
    }

    #endregion
}