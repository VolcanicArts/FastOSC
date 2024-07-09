// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
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

    public static byte[] Encode(OSCMessage message)
    {
        var index = 0;
        var data = new byte[calculateMessageLength(message)];

        insertAddress(message, data, ref index);
        insertTypeTags(message, data, ref index);
        insertArguments(message, data, ref index);

        return data;
    }

    private static int calculateMessageLength(OSCMessage message)
    {
        var totalLength = 0;

        totalLength += OSCUtils.Align(encoding.GetByteCount(message.Address));
        totalLength += OSCUtils.Align(calculateTypeTagsLength(message.Arguments));
        totalLength += calculateArgumentsLength(message.Arguments);

        return totalLength;
    }

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

    private static void insertAddress(OSCMessage message, byte[] data, ref int index)
    {
        encodeString(data, ref index, message.Address);
    }

    private static void insertTypeTags(OSCMessage message, byte[] data, ref int index)
    {
        if (message.Arguments.Length == 0) return;

        data[index++] = OSCChars.COMMA;

        insertTypeTagSymbols(message.Arguments, data, ref index);

        index = OSCUtils.Align(index);
    }

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

    private static void insertArguments(OSCMessage message, byte[] data, ref int index)
    {
        insertValues(message.Arguments, data, ref index);
    }

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

    private static void encodeInt(byte[] data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    private static void encodeFloat(byte[] data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    private static void encodeString(byte[] data, ref int index, string value)
    {
        var bytes = encoding.GetBytes(value);
        bytes.CopyTo(data, index);
        index += OSCUtils.Align(bytes.Length);
    }

    private static void encodeByteArray(byte[] data, ref int index, byte[] value)
    {
        var length = value.Length;
        encodeInt(data, ref index, length);
        value.CopyTo(data, index);

        index += OSCUtils.Align(length);
    }

    private static void encodeLong(byte[] data, ref int index, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(data.AsSpan(index, 8), value);
        index += 8;
    }

    private static void encodeDouble(byte[] data, ref int index, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(data.AsSpan(index, 8), value);
        index += 8;
    }

    private static void encodeChar(byte[] data, ref int index, char value)
    {
        var charBytes = encoding.GetBytes(new[] { value });

        for (var i = 0; i < charBytes.Length; i++)
        {
            data[index + 3 - i] = charBytes[charBytes.Length - 1 - i];
        }

        index += 4;
    }

    private static void encodeRGBA(byte[] data, ref int index, OSCRGBA value)
    {
        data[index++] = value.R;
        data[index++] = value.G;
        data[index++] = value.B;
        data[index++] = value.A;
    }

    private static void encodeMidi(byte[] data, ref int index, OSCMidi midi)
    {
        data[index++] = midi.PortID;
        data[index++] = midi.Status;
        data[index++] = midi.Data1;
        data[index++] = midi.Data2;
    }

    private static void encodeTimeTag(byte[] data, ref int index, OSCTimeTag timeTag)
    {
        BinaryPrimitives.WriteUInt64BigEndian(data.AsSpan(index, 8), timeTag.Value);
        index += 8;
    }
}
