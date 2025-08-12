// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Converting
{
    [Test]
    public static void ConvertingDateTimeToTimeTagTest()
    {
        var dateTime = DateTime.UtcNow;
        var encodedDateTime = new OSCTimeTag(dateTime);
        var decodedDateTime = encodedDateTime.ToDateTime();

        Assert.That(dateTime, Is.EqualTo(decodedDateTime).Within(TimeSpan.FromMilliseconds(1d)));
    }
}