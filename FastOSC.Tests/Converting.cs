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
}
