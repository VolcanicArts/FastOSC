// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Text;

namespace FastOSC;

public static class OSCEncoder
{
    public static byte[] Encode(OSCMessage message)
    {
        var index = 0;
        var data = new byte[calculateMessageLength(message)];

        insertAddress(message, data, ref index);
        insertTypeTags(message, data, ref index);
        insertValues(message, data, ref index);

        return data;
    }

    private static int calculateMessageLength(OSCMessage message)
    {
        var totalLength = 0;

        totalLength += OSCUtils.Align(Encoding.UTF8.GetByteCount(message.Address));
        totalLength += OSCUtils.Align(1 + message.Arguments.Length);

        foreach (var value in message.Arguments)
        {
            totalLength += value switch
            {
                string valueStr => OSCUtils.Align(Encoding.UTF8.GetByteCount(valueStr)),
                int => 4,
                float => 4,
                bool => 0,
                _ => 0
            };
        }

        return totalLength;
    }

    private static void insertAddress(OSCMessage message, byte[] data, ref int index)
    {
        stringToBytes(data, ref index, message.Address);
    }

    private static void insertTypeTags(OSCMessage message, byte[] data, ref int index)
    {
        data[index++] = OSCChars.COMMA;

        foreach (var value in message.Arguments)
        {
            data[index++] = value switch
            {
                string => OSCChars.STRING,
                int => OSCChars.INT,
                float => OSCChars.FLOAT,
                true => OSCChars.TRUE,
                false => OSCChars.FALSE,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        index = OSCUtils.Align(index);
    }

    private static void insertValues(OSCMessage message, byte[] data, ref int index)
    {
        foreach (var value in message.Arguments)
        {
            switch (value)
            {
                case int intValue:
                    intToBytes(data, ref index, intValue);
                    break;

                case float floatValue:
                    floatToBytes(data, ref index, floatValue);
                    break;

                case string stringValue:
                    stringToBytes(data, ref index, stringValue);
                    break;
            }
        }
    }

    private static void intToBytes(byte[] data, ref int index, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    private static void floatToBytes(byte[] data, ref int index, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(data.AsSpan(index, 4), value);
        index += 4;
    }

    private static void stringToBytes(byte[] data, ref int index, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        bytes.CopyTo(data, index);
        index += OSCUtils.Align(bytes.Length);
    }
}
