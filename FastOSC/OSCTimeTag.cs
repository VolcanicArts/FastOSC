namespace FastOSC;

public class OSCTimeTag : IEquatable<OSCTimeTag>
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

    public OSCTimeTag(TimeSpan timeSpan)
    {
        Value = OSCUtils.TimeSpanToTimeTag(timeSpan);
    }

    public DateTime AsDateTime() => OSCUtils.TimeTagToDateTime(Value);

    public TimeSpan AsTimeSpan() => OSCUtils.TimeTagToTimeSpan(Value);

    public bool Equals(OSCTimeTag? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((OSCTimeTag)obj);
    }

    public override int GetHashCode() => Value.GetHashCode();
}
