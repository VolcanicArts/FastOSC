namespace FastOSC.Tests;

public static class Converting
{
    [Test]
    public static void ConvertingDateTimeToTimeTagTest()
    {
        var dateTime = DateTime.UtcNow;
        var encodedDateTime = OSCUtils.DateTimeToTimeTag(dateTime);
        var decodedDateTime = OSCUtils.TimeTagToDateTime(encodedDateTime);

        Assert.That(dateTime, Is.EqualTo(decodedDateTime).Within(TimeSpan.FromMicroseconds(10)));
    }

    [Test]
    public static void ConvertingTimeSpanToTimeTagTest()
    {
        var timeSpan = new TimeSpan(1, 2, 3, 4, 5);
        var encodedTimeSpan = OSCUtils.TimeSpanToTimeTag(timeSpan);
        var decodedTimeSpan = OSCUtils.TimeTagToTimeSpan(encodedTimeSpan);

        Assert.That(timeSpan, Is.EqualTo(decodedTimeSpan).Within(TimeSpan.FromMicroseconds(10)));
    }
}
