// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Text;

namespace FastOSC;

public static class OSCEncoder
{
    private const string bundle_header = "#bundle";
    private static Encoding encoding = Encoding.ASCII;

    private static byte[] bundleHeaderBytes = encoding.GetBytes(bundle_header);

    /// <summary>
    /// This can be used to override the encoding.
    /// For example, some receivers may support UTF8 strings.
    /// </summary>
    public static void SetEncoding(Encoding newEncoding)
    {
        encoding = newEncoding;
        bundleHeaderBytes = encoding.GetBytes(bundle_header);
    }

    #region Bundle

    public static byte[] Encode(OSCBundle bundle)
    {
        var index = 0;
        var data = new byte[calculateBundleLength(bundle)];

        encodeBundle(ref data, ref index, bundle);

        return data;
    }

    private static void encodeBundle(ref byte[] data, ref int index, OSCBundle bundle)
    {
        bundleHeaderBytes.CopyTo(data, index);
        index += bundleHeaderBytes.Length;

        encodeTimeTag(ref data, ref index, bundle.TimeTag);

        foreach (var element in bundle.Elements)
        {
            switch (element)
            {
                case OSCBundle subBundle:
                    encodeInt(ref data, ref index, calculateBundleLength(subBundle));
                    encodeBundle(ref data, ref index, subBundle);
                    break;

                case OSCMessage message:
                    encodeInt(ref data, ref index, calculateMessageLength(message));
                    encodeMessage(ref data, ref index, message);
                    break;
            }
        }
    }

    private static unsafe int calculateBundleLength(OSCBundle bundle)
    {
        var totalLength = 0;

        totalLength += bundleHeaderBytes.Length;
        totalLength += sizeof(OSCTimeTag);

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

    #endregion

    #region Message

    public static byte[] Encode(OSCMessage message)
    {
        var index = 0;
        var data = new byte[calculateMessageLength(message)];

        encodeMessage(ref data, ref index, message);

        return data;
    }

    private static void encodeMessage(ref byte[] data, ref int index, OSCMessage message)
    {
        encodeString(ref data, ref index, message.Address);
        insertTypeTags(ref data, ref index, message.Arguments);
        insertArguments(ref data, ref index, message.Arguments);
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

    private static unsafe int calculateArgumentsLength(object?[] arguments)
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
                OSCTimeTag => sizeof(OSCTimeTag),
                char => 4,
                OSCRGBA => sizeof(OSCRGBA),
                OSCMidi => sizeof(OSCMidi),
                null => 0,
                bool => 0,
                object?[] subArrayArguments => calculateArgumentsLength(subArrayArguments),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return totalLength;
    }

    private static void insertTypeTags(ref byte[] data, ref int index, object?[] arguments)
    {
        if (arguments.Length == 0) return;

        data[index++] = OSCChars.COMMA;

        insertTypeTagSymbols(ref data, ref index, arguments);

        index = OSCUtils.Align(index);
    }

    private static void insertTypeTagSymbols(ref byte[] data, ref int index, object?[] arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument is object?[] arrayArguments)
            {
                data[index++] = OSCChars.ARRAY_BEGIN;
                insertTypeTagSymbols(ref data, ref index, arrayArguments);
                data[index++] = OSCChars.ARRAY_END;
                continue;
            }

            data[index++] = argument switch
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

    private static void insertArguments(ref byte[] data, ref int index, object?[] values)
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
                    encodeInt(ref data, ref index, intValue);
                    break;

                case float floatValue:
                    encodeFloat(ref data, ref index, floatValue);
                    break;

                case string stringValue:
                    encodeString(ref data, ref index, stringValue);
                    break;

                case byte[] byteArrayValue:
                    encodeByteArray(ref data, ref index, byteArrayValue);
                    break;

                case long longValue:
                    encodeLong(ref data, ref index, longValue);
                    break;

                case double doubleValue:
                    encodeDouble(ref data, ref index, doubleValue);
                    break;

                case char charValue:
                    encodeChar(ref data, ref index, charValue);
                    break;

                case OSCRGBA rgbaValue:
                    encodeRGBA(ref data, ref index, rgbaValue);
                    break;

                case OSCMidi midiValue:
                    encodeMidi(ref data, ref index, midiValue);
                    break;

                case OSCTimeTag timeTagValue:
                    encodeTimeTag(ref data, ref index, timeTagValue);
                    break;

                case object?[] subArrayArguments:
                    insertArguments(ref data, ref index, subArrayArguments);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void encodeInt(ref byte[] data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    private static void encodeFloat(ref byte[] data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    private static void encodeString(ref byte[] data, ref int index, string value)
    {
        var bytes = encoding.GetBytes(value);
        bytes.CopyTo(data, index);
        index += OSCUtils.Align(bytes.Length);
    }

    private static void encodeByteArray(ref byte[] data, ref int index, byte[] value)
    {
        var length = value.Length;
        encodeInt(ref data, ref index, length);
        value.CopyTo(data, index);

        index += OSCUtils.Align(length);
    }

    private static void encodeLong(ref byte[] data, ref int index, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(data.AsSpan(index, 8), value);
        index += 8;
    }

    private static void encodeDouble(ref byte[] data, ref int index, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(data.AsSpan(index, 8), value);
        index += 8;
    }

    private static void encodeChar(ref byte[] data, ref int index, char value)
    {
        var charBytes = encoding.GetBytes(new[] { value });

        for (var i = 0; i < charBytes.Length; i++)
        {
            data[index + 3 - i] = charBytes[charBytes.Length - 1 - i];
        }

        index += 4;
    }

    private static void encodeRGBA(ref byte[] data, ref int index, OSCRGBA value)
    {
        data[index++] = value.R;
        data[index++] = value.G;
        data[index++] = value.B;
        data[index++] = value.A;
    }

    private static void encodeMidi(ref byte[] data, ref int index, OSCMidi midi)
    {
        data[index++] = midi.PortID;
        data[index++] = midi.Status;
        data[index++] = midi.Data1;
        data[index++] = midi.Data2;
    }

    private static void encodeTimeTag(ref byte[] data, ref int index, OSCTimeTag timeTag)
    {
        BinaryPrimitives.WriteUInt64BigEndian(data.AsSpan(index, 8), timeTag.Value);
        index += 8;
    }

    #endregion
}