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
        var end = Align(index);
        for (; index < end; index++) data[index] = 0;
    }

    /// <summary>
    /// Finds a byte at or above <paramref name="index"/>, or <paramref name="data"/>'s length if the end of the sequence is reached
    /// </summary>
    public static int FindByteIndex(ReadOnlySpan<byte> data, int index, byte target = 0)
    {
        var length = data.Length;
        if (index >= length) throw new IndexOutOfRangeException($"{nameof(index)} is out of bounds for array {nameof(data)}");

        var found = data[index..].IndexOf(target);
        return found == -1 ? length : index + found;
    }
}