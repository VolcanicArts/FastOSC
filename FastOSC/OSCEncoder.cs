// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastOSC;

public static class OSCEncoder
{
    private static Encoding encoding = Encoding.ASCII;

    /// <summary>
    /// This can be used to override the encoding.
    /// For example, some receivers may support UTF8 strings.
    /// </summary>
    public static void SetEncoding(Encoding encoding)
    {
        OSCEncoder.encoding = encoding;
    }

    #region Bundle

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static byte[] Encode(OSCBundle bundle)
    {
        var index = 0;
        var data = new byte[calculateBundleLength(bundle)];

        encodeBundle(bundle, data, ref index);

        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeBundle(OSCBundle bundle, byte[] data, ref int index)
    {
        insertBundleHeader(data, ref index);
        insertBundleTimeTag(bundle, data, ref index);

        foreach (var element in bundle.Elements)
        {
            switch (element)
            {
                case OSCBundle nestedBundle:
                    encodeInt(data, ref index, calculateBundleLength(nestedBundle));
                    encodeBundle(nestedBundle, data, ref index);
                    break;

                case OSCMessage message:
                    encodeInt(data, ref index, calculateMessageLength(message));
                    encodeMessage(message, data, ref index);
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int calculateBundleLength(OSCBundle bundle)
    {
        var totalLength = 0;

        totalLength += 8; // #encode
        totalLength += 8; // timetag

        foreach (var element in bundle.Elements)
        {
            totalLength += element switch
            {
                OSCBundle nestedBundle => calculateBundleLength(nestedBundle),
                OSCMessage message => calculateMessageLength(message),
                _ => throw new ArgumentOutOfRangeException()
            };

            totalLength += 4; // length
        }

        return totalLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertBundleHeader(byte[] data, ref int index)
    {
        var headerText = encoding.GetBytes("#bundle");
        headerText.CopyTo(data, index);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertBundleTimeTag(OSCBundle bundle, byte[] data, ref int index)
    {
        encodeTimeTag(data, ref index, bundle.TimeTag);
    }

    #endregion

    #region Message

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static byte[] Encode(OSCMessage message)
    {
        var index = 0;
        var data = new byte[calculateMessageLength(message)];

        encodeMessage(message, data, ref index);

        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeMessage(OSCMessage message, byte[] data, ref int index)
    {
        insertAddress(message, data, ref index);
        insertTypeTags(message, data, ref index);
        insertArguments(message, data, ref index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int calculateMessageLength(OSCMessage message)
    {
        var totalLength = 0;

        totalLength += OSCUtils.Align(encoding.GetByteCount(message.Address));
        totalLength += OSCUtils.Align(calculateTypeTagsLength(message.Arguments));
        totalLength += calculateArgumentsLength(message.Arguments);

        return totalLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int calculateTypeTagsLength(object?[] arguments)
    {
        var totalLength = 1;

        foreach (var argument in arguments)
        {
            if (argument is object?[] internalArrayValue)
                totalLength += calculateTypeTagsLength(internalArrayValue) + 2;
            else
                totalLength += 1;
        }

        return totalLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int calculateArgumentsLength(object?[] arguments)
    {
        var totalLength = 0;

        foreach (var value in arguments)
        {
            totalLength += value switch
            {
                string valueStr => OSCUtils.Align(encoding.GetByteCount(valueStr)),
                int => 4,
                float.PositiveInfinity => 0,
                float => 4,
                byte[] valueByteArray => 4 + OSCUtils.Align(valueByteArray.Length, false),
                long => 8,
                double => 8,
                OSCTimeTag => 8,
                char => 4,
                OSCRGBA => 4,
                OSCMidi => 4,
                null => 0,
                bool => 0,
                object?[] valueInternalArray => calculateArgumentsLength(valueInternalArray),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return totalLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertAddress(OSCMessage message, byte[] data, ref int index)
    {
        encodeString(data, ref index, message.Address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertTypeTags(OSCMessage message, byte[] data, ref int index)
    {
        if (message.Arguments.Length == 0) return;

        data[index++] = OSCChars.COMMA;

        insertTypeTagSymbols(message.Arguments, data, ref index);

        index = OSCUtils.Align(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertTypeTagSymbols(object?[] arguments, byte[] data, ref int index)
    {
        foreach (var value in arguments)
        {
            if (value is object?[] nestedArrayValue)
            {
                data[index++] = OSCChars.ARRAY_BEGIN;
                insertTypeTagSymbols(nestedArrayValue, data, ref index);
                data[index++] = OSCChars.ARRAY_END;
                continue;
            }

            data[index++] = value switch
            {
                string => OSCChars.STRING,
                int => OSCChars.INT,
                float.PositiveInfinity => OSCChars.INFINITY,
                float => OSCChars.FLOAT,
                true => OSCChars.TRUE,
                false => OSCChars.FALSE,
                byte[] => OSCChars.BLOB,
                long => OSCChars.LONG,
                double => OSCChars.DOUBLE,
                char => OSCChars.CHAR,
                null => OSCChars.NIL,
                OSCRGBA => OSCChars.RGBA,
                OSCMidi => OSCChars.MIDI,
                OSCTimeTag => OSCChars.TIMETAG,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertArguments(OSCMessage message, byte[] data, ref int index)
    {
        insertValues(message.Arguments, data, ref index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void insertValues(object?[] values, byte[] data, ref int index)
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

                case object?[] internalArrayValues:
                    insertValues(internalArrayValues, data, ref index);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeInt(byte[] data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeFloat(byte[] data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeString(byte[] data, ref int index, string value)
    {
        var bytes = encoding.GetBytes(value);
        bytes.CopyTo(data, index);
        index += OSCUtils.Align(bytes.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeByteArray(byte[] data, ref int index, byte[] value)
    {
        var length = value.Length;
        encodeInt(data, ref index, length);
        value.CopyTo(data, index);

        index += OSCUtils.Align(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeLong(byte[] data, ref int index, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(data.AsSpan(index, 8), value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeDouble(byte[] data, ref int index, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(data.AsSpan(index, 8), value);
        index += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeChar(byte[] data, ref int index, char value)
    {
        var charBytes = encoding.GetBytes(new[] { value });

        for (var i = 0; i < charBytes.Length; i++)
        {
            data[index + 3 - i] = charBytes[charBytes.Length - 1 - i];
        }

        index += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeRGBA(byte[] data, ref int index, OSCRGBA value)
    {
        data[index++] = value.R;
        data[index++] = value.G;
        data[index++] = value.B;
        data[index++] = value.A;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeMidi(byte[] data, ref int index, OSCMidi midi)
    {
        data[index++] = midi.PortID;
        data[index++] = midi.Status;
        data[index++] = midi.Data1;
        data[index++] = midi.Data2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static void encodeTimeTag(byte[] data, ref int index, OSCTimeTag timeTag)
    {
        BinaryPrimitives.WriteUInt64BigEndian(data.AsSpan(index, 8), timeTag.Value);
        index += 8;
    }

    #endregion
}