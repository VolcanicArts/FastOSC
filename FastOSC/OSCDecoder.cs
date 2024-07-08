// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers.Binary;
using System.Text;

namespace FastOSC;

public static class OSCDecoder
{
    public static OSCMessage? Decode(byte[] data)
    {
        var index = 0;

        var address = decodeAddress(data, ref index);

        if (address is null)
            return null;

        index = OSCUtils.Align(index);

        var typeTags = decodeTypeTags(data, ref index);

        if (typeTags.Length == 0)
            return null;

        index = OSCUtils.Align(index);

        var values = decodeAttributes(typeTags, data, ref index);

        return new OSCMessage(address, values);
    }

    private static string? decodeAddress(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OSCChars.SLASH) return null;

        while (data[index] != 0) index++;

        return Encoding.UTF8.GetString(data.AsSpan(start, index - start));
    }

    private static Span<byte> decodeTypeTags(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OSCChars.COMMA) return Array.Empty<byte>();

        while (data[index] != 0) index++;

        return data.AsSpan(start + 1, index - (start + 1));
    }

    private static object[] decodeAttributes(Span<byte> typeTags, byte[] msg, ref int index)
    {
        var values = new object[typeTags.Length];

        for (var i = 0; i < typeTags.Length; i++)
        {
            var type = typeTags[i];

            values[i] = type switch
            {
                OSCChars.INT => bytesToInt(msg, ref index),
                OSCChars.FLOAT => bytesToFloat(msg, ref index),
                OSCChars.STRING => bytesToString(msg, ref index),
                OSCChars.TRUE => true,
                OSCChars.FALSE => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return values;
    }

    private static int bytesToInt(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(index, 4));
        index += 4;
        return value;
    }

    private static float bytesToFloat(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadSingleBigEndian(data.AsSpan(index, 4));
        index += 4;
        return value;
    }

    private static string bytesToString(byte[] data, ref int index)
    {
        var start = index;
        while (data[index] != 0) index++;

        var stringData = Encoding.UTF8.GetString(data.AsSpan(start, index - start));
        index = OSCUtils.Align(index);
        return stringData;
    }
}
