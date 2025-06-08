// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public readonly record struct OSCMidi(byte PortID, byte Status, byte Data1, byte Data2)
{
    public void Deconstruct(out byte portId, out byte status, out byte data1, out byte data2)
    {
        portId = PortID;
        status = Status;
        data1 = Data1;
        data2 = Data2;
    }
}

public readonly record struct OSCRGBA(byte R, byte G, byte B, byte A)
{
    public void Deconstruct(out byte r, out byte g, out byte b, out byte a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }
}

public readonly record struct OSCTimeTag
{
    private readonly ulong value;

    public OSCTimeTag(ulong value)
    {
        this.value = value;
    }

    public OSCTimeTag(DateTime dateTime)
    {
        value = fromDateTime(dateTime);
    }

    public static explicit operator ulong(OSCTimeTag timeTag) => timeTag.value;

    public static explicit operator DateTime(OSCTimeTag timeTag) => toDateTime(timeTag.value);

    private static ulong fromDateTime(DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
            dateTime = dateTime.ToUniversalTime();

        var timeSinceOscEpoch = dateTime - OSCConst.OSC_EPOCH;

        var seconds = (uint)timeSinceOscEpoch.TotalSeconds;
        var fractionalPart = timeSinceOscEpoch.TotalSeconds - seconds;
        var fractional = (uint)(fractionalPart * (1L << 32));

        return (ulong)seconds << 32 | fractional;
    }

    private static DateTime toDateTime(ulong timeTag)
    {
        var seconds = (uint)(timeTag >> 32);
        var fractional = (uint)(timeTag & 0xFFFFFFFF);
        var fractionalSeconds = fractional / (double)(1L << 32);
        return OSCConst.OSC_EPOCH.AddSeconds(seconds + fractionalSeconds);
    }
}