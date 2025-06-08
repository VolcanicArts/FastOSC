// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public static class OSCUtils
{
    /// <summary>
    /// Aligns an index to an interval of 4.
    /// If the index is already aligned, <paramref name="alignEvenIfAligned"/> controls whether to add 4 anyway
    /// </summary>
    public static int Align(int index, bool alignEvenIfAligned = true) => alignEvenIfAligned ? index + (4 - index % 4) : index % 4 != 0 ? index + index % 4 : index;

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