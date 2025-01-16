// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public readonly record struct OSCMidi(byte PortID, byte Status, byte Data1, byte Data2);

public readonly record struct OSCRGBA(byte R, byte G, byte B, byte A);

public readonly record struct OSCTimeTag
{
    public readonly ulong Value;

    public OSCTimeTag(ulong value)
    {
        Value = value;
    }

    public OSCTimeTag(DateTime dateTime)
    {
        Value = OSCUtils.DateTimeToTimeTag(dateTime);
    }

    public DateTime AsDateTime() => OSCUtils.TimeTagToDateTime(Value);
}
