// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Decoding
{
    private const string test_string = "/tst";

    [Test]
    public static void DecodingNullTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.NIL, 0x0, 0x0 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { null }));
        });
    }

    [Test]
    public static void DecodingInfinityTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.INFINITY, 0x0, 0x0 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { float.PositiveInfinity }));
        });
    }

    [Test]
    public static void DecodingBoolTrueTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.TRUE, 0x0, 0x0 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { true }));
        });
    }

    [Test]
    public static void DecodingBoolFalseTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.FALSE, 0x0, 0x0 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { false }));
        });
    }

    [Test]
    public static void DecodingIntTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.INT, 0x0, 0x0, 0x0, 0x0, 0x0, 0x01 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { 1 }));
        });
    }

    [Test]
    public static void DecodingFloatTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.FLOAT, 0x0, 0x0, 0x3F, 0x80, 0x00, 0x00 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { 1f }));
        });
    }

    [Test]
    public static void DecodingLongTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.LONG, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x01 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { 1L }));
        });
    }

    [Test]
    public static void DecodingDoubleTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.DOUBLE, 0x0, 0x0, 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { 1d }));
        });
    }

    [Test]
    public static void DecodingStringTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.STRING, 0x0, 0x0, 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { test_string }));
        });
    }

    [Test]
    public static void DecodingBlobTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.BLOB, 0x0, 0x0, 0x00, 0x00, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { new byte[] { 0x1, 0x2, 0x3, 0x4 } }));
        });
    }

    [Test]
    public static void DecodingCharTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.CHAR, 0x0, 0x0, 0x00, 0x00, 0x00, 0x61 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { 'a' }));
        });
    }

    [Test]
    public static void DecodingRGBATest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.RGBA, 0x0, 0x0, 0x01, 0x02, 0x03, 0x04 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { new OSCRGBA(1, 2, 3, 4) }));
        });
    }

    [Test]
    public static void DecodingMidiTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.MIDI, 0x0, 0x0, 0x01, 0x02, 0x03, 0x04 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { new OSCMidi(1, 2, 3, 4) }));
        });
    }

    [Test]
    public static void DecodingTimeTagTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.TIMETAG, 0x0, 0x0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xD2 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { new OSCTimeTag(1234ul) }));
        });
    }

    [Test]
    public static void DecodingArrayTest()
    {
        var message = OSCDecoder.Decode(new byte[]
            { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCChars.COMMA, OSCChars.ARRAY_BEGIN, OSCChars.INT, OSCChars.ARRAY_END, 0x0, 0x0, 0x0, 0x0, 0x00, 0x00, 0x00, 0x01 });

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { new object?[] { 1 } }));
        });
    }
}
