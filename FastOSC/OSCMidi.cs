// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.InteropServices;

namespace FastOSC;

[StructLayout(LayoutKind.Sequential)]
public readonly struct OSCMIDI
{
    public readonly byte PortID;
    public readonly byte Status;
    public readonly byte Data1;
    public readonly byte Data2;

    public OSCMIDIStatus StatusType => (OSCMIDIStatus)(Status >= 0xF0 ? Status : Status & 0xF0);
    public int StatusChannel => Status >= 0xF0 ? -1 : Status & 0x0F;

    internal OSCMIDI(byte portID, byte status, byte data1, byte data2)
    {
        PortID = portID;
        Status = status;
        Data1 = data1;
        Data2 = data2;
    }

    internal OSCMIDI(byte portID, OSCMIDIStatus status, byte data1, byte data2)
    {
        PortID = portID;
        Status = (byte)status;
        Data1 = data1;
        Data2 = data2;
    }

    public override string ToString() => $"PortID: {PortID:X2} | Status: {Status:X2} | Data1: {Data1:X2} | Data2: {Data2:X2}";
}

/// <summary>
/// MIDI 1.0 status bytes. Status type uses the high nibble, status channel uses the low nibble (0–15).
/// System messages are fixed single-byte values.
/// </summary>
public enum OSCMIDIStatus : byte
{
    NoteOff = 0x80,
    NoteOn = 0x90,
    PolyphonicAftertouch = 0xA0,
    ControlChange = 0xB0,
    ProgramChange = 0xC0,
    ChannelAftertouch = 0xD0,
    PitchBendChange = 0xE0,
    SystemExclusive = 0xF0,
    MidiTimeCodeQtrFrame = 0xF1,
    SongPositionPointer = 0xF2,
    SongSelect = 0xF3,
    TuneRequest = 0xF6,
    EndOfSysEx = 0xF7,
    TimingClock = 0xF8,
    Start = 0xFA,
    Continue = 0xFB,
    Stop = 0xFC,
    ActiveSensing = 0xFE,
    SystemReset = 0xFF
}

public static class OSCMidiStatusExtensions
{
    /// <summary>
    /// Combines a status type and a channel (0–15) into a status byte.
    /// </summary>
    public static byte OnChannel(this OSCMIDIStatus type, int channel) => (byte)((byte)type | channel & 0x0F);
}