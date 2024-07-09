namespace FastOSC;

public class OSCTimeTag
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
}
