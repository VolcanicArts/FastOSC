// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using System.Runtime.InteropServices;

namespace FastOSC;

public static class OSC
{
    public static readonly DateTime EPOCH = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly OSCNil NIL = new();
    public static readonly OSCInfinitum INFINITUM = new();

    public static OSCMIDI MIDI(byte portID, byte status, byte data1, byte data2) => new(portID, status, data1, data2);
    public static OSCMIDI MIDI(byte portID, OSCMIDIStatus status, byte data1, byte data2) => new(portID, status, data1, data2);
    public static OSCRGBA RGBA(byte r, byte g, byte b, byte a) => new(r, g, b, a);
    public static OSCTimeTag TimeTag(ulong value) => new(value);
    public static OSCTimeTag TimeTag(DateTime dateTime) => new(dateTime);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCNil;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCInfinitum;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCRGBA
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    internal OSCRGBA(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public override string ToString() => $"{R:X2}{G:X2}{B:X2}{A:X2}";
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCTimeTag
{
    public readonly ulong Value;

    internal OSCTimeTag(ulong value)
    {
        Value = value;
    }

    internal OSCTimeTag(DateTime dateTime)
    {
        Value = fromDateTime(dateTime);
    }

    public override string ToString() => ToDateTime().ToString(CultureInfo.InvariantCulture);

    public DateTime ToDateTime()
    {
        var seconds = (uint)(Value >> 32);
        var fractional = (uint)(Value & 0xFFFFFFFF);
        var fractionalSeconds = fractional / (double)(1L << 32);
        return OSC.EPOCH.AddSeconds(seconds + fractionalSeconds);
    }

    private static ulong fromDateTime(DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
            dateTime = dateTime.ToUniversalTime();

        var timeSinceOscEpoch = dateTime - OSC.EPOCH;

        var seconds = (uint)timeSinceOscEpoch.TotalSeconds;
        var fractionalPart = timeSinceOscEpoch.TotalSeconds - seconds;
        var fractional = (uint)(fractionalPart * (1L << 32));

        return (ulong)seconds << 32 | fractional;
    }
}