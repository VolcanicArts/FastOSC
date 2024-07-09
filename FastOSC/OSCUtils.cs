// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;

namespace FastOSC;

public static class OSCUtils
{
    private static readonly DateTime osc_epoch = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Aligns an index to an interval of 4.
    /// If the index is already aligned, <paramref name="alignEvenIfAligned"/> controls whether to add 4 anyway
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Align(int index, bool alignEvenIfAligned = true) => alignEvenIfAligned ? index + (4 - index % 4) : index % 4 != 0 ? index + (index % 4) : index;

    /// <summary>
    /// Finds a byte at or above <paramref name="index"/>, or <paramref name="data"/>.Length if the end of the sequence is reached
    /// </summary>
    public static int FindByteIndex(byte[] data, int index, byte target = 0)
    {
        if (index >= data.Length) throw new IndexOutOfRangeException($"{nameof(index)} is out of bounds for array {nameof(data)}");

        while (data[index] != target && index < data.Length)
        {
            index++;
        }

        return index;
    }

    /// <summary>
    /// Finds a byte at or above <paramref name="index"/>, or <paramref name="data"/>.Length if the end of the sequence is reached
    /// </summary>
    public static int FindByteIndex(Span<byte> data, int index, byte target = 0)
    {
        if (index >= data.Length) throw new IndexOutOfRangeException($"{nameof(index)} is out of bounds for array {nameof(data)}");

        while (data[index] != target && index < data.Length)
        {
            index++;
        }

        return index;
    }

    public static ulong DateTimeToTimeTag(DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
            dateTime = dateTime.ToUniversalTime();

        var timeSinceOscEpoch = dateTime - osc_epoch;
        return TimeSpanToTimeTag(timeSinceOscEpoch);
    }

    public static ulong TimeSpanToTimeTag(TimeSpan timeSpan)
    {
        var seconds = (uint)timeSpan.TotalSeconds;
        var fractionalPart = timeSpan.TotalSeconds - seconds;
        var fractional = (uint)(fractionalPart * (1L << 32));

        var timeTag = ((ulong)seconds << 32) | fractional;

        return timeTag;
    }

    public static DateTime TimeTagToDateTime(ulong timeTag)
    {
        var seconds = (uint)(timeTag >> 32);
        var fractional = (uint)(timeTag & 0xFFFFFFFF);

        var fractionalSeconds = fractional / (double)(1L << 32);

        var dateTime = osc_epoch.AddSeconds(seconds + fractionalSeconds);

        return dateTime;
    }

    public static TimeSpan TimeTagToTimeSpan(ulong timeTag)
    {
        var seconds = (uint)(timeTag >> 32);
        var fractional = (uint)(timeTag & 0xFFFFFFFF);

        var fractionalSeconds = fractional / (double)(1L << 32);

        var timeSpan = TimeSpan.FromSeconds(seconds + fractionalSeconds);

        return timeSpan;
    }
}
