// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;

namespace FastOSC;

public static class OSCUtils
{
    /// <summary>
    /// Aligns an index to an interval of 4.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Align(int index) => index + 3 & ~3;

    /// <summary>
    /// Aligns an index to an interval of 4 and writes nulls where needed
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AlignAndWriteNulls(Span<byte> data, ref int index, bool includeNullTerminator)
    {
        if (includeNullTerminator) data[index++] = 0;
        var pad = Align(index) - index;
        if (pad >= 1) data[index++] = 0;
        if (pad >= 2) data[index++] = 0;
        if (pad == 3) data[index++] = 0;
    }

    /// <summary>
    /// Finds the null byte's index at or above <paramref name="index"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindNullTerminator(ReadOnlySpan<byte> data, int index)
    {
        var length = data.Length;
        if (index >= length) throw new IndexOutOfRangeException($"{nameof(index)} is out of bounds for array {nameof(data)}");

        var found = data[index..].IndexOf((byte)0);
        if (found == -1) throw new IndexOutOfRangeException("Could not find null terminator");

        return index + found;
    }

    /// <summary>
    /// Given tags[...] where tags[beginIndex] == '[', returns the matching ']' index
    /// </summary>
    public static int FindMatchingArrayEnd(ReadOnlySpan<byte> tags, int beginIndex)
    {
        if (beginIndex < 0 || beginIndex >= tags.Length || tags[beginIndex] != OSCConst.ARRAY_BEGIN)
            throw new ArgumentOutOfRangeException(nameof(beginIndex));

        var depth = 1;

        for (int i = beginIndex + 1; i < tags.Length; i++)
        {
            var t = tags[i];

            switch (t)
            {
                case OSCConst.ARRAY_BEGIN:
                    depth++;
                    break;

                case OSCConst.ARRAY_END when --depth == 0:
                    return i;
            }
        }

        throw new FormatException("Unbalanced array brackets in type tags");
    }
}