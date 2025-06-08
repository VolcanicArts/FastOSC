// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Decoder
{
    private const string test_string = "/tst";

    [Test]
    public static void DecodingNullTest()
    {
        var message = OSCDecoder.Decode("/tst\0\0\0\0,N\0\0"u8.ToArray()) as OSCMessage;

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
        var message = OSCDecoder.Decode("/tst\0\0\0\0,I\0\0"u8.ToArray()) as OSCMessage;

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
        var message = OSCDecoder.Decode("/tst\0\0\0\0,T\0\0"u8.ToArray()) as OSCMessage;

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
        var message = OSCDecoder.Decode("/tst\0\0\0\0,F\0\0"u8.ToArray()) as OSCMessage;

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
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.INT, 0x0, 0x0, 0x0, 0x0, 0x0, 0x01 }) as OSCMessage;

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
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.FLOAT, 0x0, 0x0, 0x3F, 0x80, 0x00, 0x00 }) as OSCMessage;

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
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.LONG, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x01 }) as OSCMessage;

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
        var message =
            OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.DOUBLE, 0x0, 0x0, 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }) as OSCMessage;

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
        var message = OSCDecoder.Decode("/tst\0\0\0\0,s\0\0/tst\0\0\0\0"u8.ToArray()) as OSCMessage;

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
        var message =
            OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.BLOB, 0x0, 0x0, 0x00, 0x00, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04 }) as OSCMessage;

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
        var message = OSCDecoder.Decode("/tst\0\0\0\0,c\0\0\0\0\0a"u8.ToArray()) as OSCMessage;

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
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.RGBA, 0x0, 0x0, 0x01, 0x02, 0x03, 0x04 }) as OSCMessage;

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
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.MIDI, 0x0, 0x0, 0x01, 0x02, 0x03, 0x04 }) as OSCMessage;

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
        var message =
            OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.TIMETAG, 0x0, 0x0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xD2 }) as OSCMessage;

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
            { 0x2F, 0x74, 0x73, 0x74, 0x0, 0x0, 0x0, 0x0, OSCConst.COMMA, OSCConst.ARRAY_BEGIN, OSCConst.INT, OSCConst.ARRAY_END, 0x0, 0x0, 0x0, 0x0, 0x00, 0x00, 0x00, 0x01 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object?[] { new object?[] { 1 } }));
        });
    }

    [Test]
    public static void DecodingBundleTest()
    {
        var message = OSCDecoder.Decode("#bundle\0\0\0\0\0\0\0\0\0\0\0\0\u0010/tst\0\0\0\0,i\0\0\0\0\0\u0001\0\0\0\u0010/ts2\0\0\0\0,i\0\0\0\0\0\u0002"u8) as OSCBundle;

        Assert.That(message, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(message.TimeTag, Is.EqualTo(new OSCTimeTag(OSCConst.OSC_EPOCH)));
            Assert.That(message.Packets, Has.Length.EqualTo(2));
            Assert.That(((OSCMessage)message.Packets[0]).Address, Is.EqualTo("/tst"));
            Assert.That(((OSCMessage)message.Packets[0]).Arguments[0], Is.EqualTo(1));
            Assert.That(((OSCMessage)message.Packets[1]).Address, Is.EqualTo("/ts2"));
            Assert.That(((OSCMessage)message.Packets[1]).Arguments[0], Is.EqualTo(2));
        });
    }
}