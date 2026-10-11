// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Converting
{
    [Test]
    public static void ConvertingDateTimeToTimeTagTest()
    {
        var dateTime = DateTime.UtcNow;
        var encodedDateTime = OSC.TimeTag(dateTime);
        var decodedDateTime = encodedDateTime.ToDateTime();

        Assert.That(dateTime, Is.EqualTo(decodedDateTime).Within(TimeSpan.FromMilliseconds(1d)));
    }

    [Test]
    public static void DecodeToEncodeMatchTest()
    {
        var message = new OSCMessage("/tst", 0.5f);
        var encodedBytes = OSCEncoder.Encode(message);
        var decodedMessage = (OSCMessage)OSCDecoder.Decode(encodedBytes);
        var doubleEncodedBytes = OSCEncoder.Encode(decodedMessage);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(doubleEncodedBytes, Is.EqualTo(encodedBytes));
            Assert.That(decodedMessage.TypeTagsLength, Is.EqualTo(message.TypeTagsLength));
            Assert.That(decodedMessage.ArgumentsLength, Is.EqualTo(message.ArgumentsLength));
        }
    }
}