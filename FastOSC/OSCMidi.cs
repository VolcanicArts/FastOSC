// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

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
