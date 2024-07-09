namespace FastOSC;

public class OSCMidi : IEquatable<OSCMidi>
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

    public bool Equals(OSCMidi? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return PortID == other.PortID && Status == other.Status && Data1 == other.Data1 && Data2 == other.Data2;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((OSCMidi)obj);
    }

    public override int GetHashCode() => HashCode.Combine(PortID, Status, Data1, Data2);
}
