// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Encoding
{
    private const string test_string = "/tst";

    [Test]
    public static void EncodingNullTest()
    {
        var message = new OSCMessage(test_string, new object?[] { null });
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,N\0\0"u8.ToArray()));
    }

    [Test]
    public static void EncodingInfinityTest()
    {
        var message = new OSCMessage(test_string, float.PositiveInfinity);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,I\0\0"u8.ToArray()));
    }

    [Test]
    public static void EncodingBoolTrueTest()
    {
        var message = new OSCMessage(test_string, true);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,T\0\0"u8.ToArray()));
    }

    [Test]
    public static void EncodingBoolFalseTest()
    {
        var message = new OSCMessage(test_string, false);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,F\0\0"u8.ToArray()));
    }

    [Test]
    public static void EncodingIntTest()
    {
        var message = new OSCMessage(test_string, 1);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.INT, 0x0, 0x0, 0x0, 0x0, 0x0, 0x01 }));
    }

    [Test]
    public static void EncodingFloatTest()
    {
        var message = new OSCMessage(test_string, 1f);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.FLOAT, 0x0, 0x0, 0x3F, 0x80, 0x00, 0x00 }));
    }

    [Test]
    public static void EncodingLongTest()
    {
        var message = new OSCMessage(test_string, 1L);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.LONG, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x01 }));
    }

    [Test]
    public static void EncodingDoubleTest()
    {
        var message = new OSCMessage(test_string, 1d);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.DOUBLE, 0x0, 0x0, 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
    }

    [Test]
    public static void EncodingStringTest()
    {
        var message = new OSCMessage(test_string, test_string);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,s\0\0/tst\0\0\0\0"u8.ToArray()));
    }

    [Test]
    public static void EncodingBlobTest()
    {
        var message = new OSCMessage(test_string, new byte[] { 0x1, 0x2, 0x3, 0x4 });
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.BLOB, 0x0, 0x0, 0x00, 0x00, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04 }));
    }

    [Test]
    public static void EncodingCharTest()
    {
        var message = new OSCMessage(test_string, 'a');
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,c\0\0\0\0\0a"u8.ToArray()));
    }

    [Test]
    public static void EncodingRGBATest()
    {
        var message = new OSCMessage(test_string, new OSCRGBA(1, 2, 3, 4));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.RGBA, 0x0, 0x0, 0x01, 0x02, 0x03, 0x04 }));
    }

    [Test]
    public static void EncodingMidiTest()
    {
        var message = new OSCMessage(test_string, new OSCMidi(1, 2, 3, 4));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.MIDI, 0x0, 0x0, 0x01, 0x02, 0x03, 0x04 }));
    }

    [Test]
    public static void EncodingTimeTagTest()
    {
        var message = new OSCMessage(test_string, new OSCTimeTag(1234ul));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.TIMETAG, 0x0, 0x0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xD2 }));
    }

    [Test]
    public static void EncodingArrayTest()
    {
        var message = new OSCMessage(test_string, new object?[] { new object?[] { 1 } });
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData,
            Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.ARRAY_BEGIN, OSCChars.INT, OSCChars.ARRAY_END, 0x0, 0x0, 0x0, 0x0, 0x00, 0x00, 0x00, 0x01 }));
    }

    [Test]
    public static void EncodingNestedArrayTest()
    {
        var message = new OSCMessage(test_string, new object?[] { new object?[] { new object[] { new object[] { 1 } } } });
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[]
        {
            0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.ARRAY_BEGIN, OSCChars.ARRAY_BEGIN, OSCChars.ARRAY_BEGIN, OSCChars.INT, OSCChars.ARRAY_END, OSCChars.ARRAY_END,
            OSCChars.ARRAY_END, 0x0, 0x0, 0x0, 0x0, 0x00, 0x00, 0x00, 0x01
        }));
    }
}