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
    public static void AlignAndWriteNulls(Span<byte> data, ref int index)
    {
        var pad = Align(index) - index;

        switch (pad)
        {
            case 3:
                data[index + 2] = 0;
                goto case 2;

            case 2:
                data[index + 1] = 0;
                goto case 1;

            case 1:
                data[index] = 0;
                break;
        }

        index += pad;
    }

    /// <summary>
    /// Aligns an index to an interval of 4 and writes nulls where needed, with an included null terminator at <paramref name="index"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AlignAndWriteNullsWithTerminator(Span<byte> data, ref int index)
    {
        data[index++] = 0;
        AlignAndWriteNulls(data, ref index);
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
        if (beginIndex < 0 || beginIndex >= tags.Length || tags[beginIndex] != OSCChar.ARRAY_BEGIN)
            throw new ArgumentOutOfRangeException(nameof(beginIndex));

        var depth = 1;

        for (var i = beginIndex + 1; i < tags.Length; i++)
        {
            var t = tags[i];

            switch (t)
            {
                case OSCChar.ARRAY_BEGIN:
                    depth++;
                    break;

                case OSCChar.ARRAY_END when --depth == 0:
                    return i;
            }
        }

        throw new FormatException("Unbalanced array brackets in type tags");
    }
}