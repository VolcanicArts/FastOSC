// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;

namespace FastOSC;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCMidi
{
    public readonly byte PortID;
    public readonly byte Status;
    public readonly byte Data1;
    public readonly byte Data2;

    public OSCMidi(byte portID, byte status, byte data1, byte data2)
    {
        PortID = portID;
        Status = status;
        Data1 = data1;
        Data2 = data2;
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCRGBA
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    public OSCRGBA(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCTimeTag
{
    public readonly ulong Value;

    public OSCTimeTag(ulong value)
    {
        Value = value;
    }

    public OSCTimeTag(DateTime dateTime)
    {
        Value = fromDateTime(dateTime);
    }

    public DateTime ToDateTime()
    {
        var seconds = (uint)(Value >> 32);
        var fractional = (uint)(Value & 0xFFFFFFFF);
        var fractionalSeconds = fractional / (double)(1L << 32);
        return OSCConst.OSC_EPOCH.AddSeconds(seconds + fractionalSeconds);
    }

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
}