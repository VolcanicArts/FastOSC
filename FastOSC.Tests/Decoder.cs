// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Decoder
{
    private const string test_string = "/tst";

    [Test]
    public static void DecodingNilTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x4E, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { OSC.NIL }));
        }
    }

    [Test]
    public static void DecodingInfinityTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x49, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { OSC.INFINITUM }));
        }
    }

    [Test]
    public static void DecodingBoolTrueTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x54, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { true }));
        }
    }

    [Test]
    public static void DecodingBoolFalseTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x46, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { false }));
        }
    }

    [Test]
    public static void DecodingIntTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { 1 }));
        }
    }

    [Test]
    public static void DecodingFloatTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x66, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { 1f }));
        }
    }

    [Test]
    public static void DecodingFloat3Test()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x66, 0x66, 0x66, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { 1f, 1f, 1f }));
        }
    }

    [Test]
    public static void DecodingLongTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { 1L }));
        }
    }

    [Test]
    public static void DecodingDoubleTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x64, 0x00, 0x00, 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { 1d }));
        }
    }

    [Test]
    public static void DecodingStringTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x73, 0x6F, 0x6D, 0x65, 0x20, 0x74, 0x65, 0x73, 0x74, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x00, 0x00, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { "some test string" }));
        }
    }

    [Test]
    public static void DecodingBlobTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { new byte[] { 0x1, 0x2, 0x3, 0x4 } }));
        }
    }

    [Test]
    public static void DecodingCharTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x63, 0x00, 0x00, 0x61, 0x00, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { 'a' }));
        }
    }

    [Test]
    public static void DecodingRGBATest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x72, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { OSC.RGBA(1, 2, 3, 4) }));
        }
    }

    [Test]
    public static void DecodingMIDITest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x6D, 0x00, 0x00, 0x01, 0xF0, 0x03, 0x04 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { OSC.MIDI(1, OSCMIDIStatus.SystemExclusive, 3, 4) }));
        }
    }

    [Test]
    public static void DecodingTimeTagTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x74, 0x00, 0x00, 0x83, 0xAA, 0x7E, 0x80, 0x00, 0x00, 0x00, 0x00 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Is.EqualTo(new object[] { OSC.TimeTag(DateTime.UnixEpoch) }));
        }
    }

    [Test]
    public static void DecodingArrayTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x5B, 0x69, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }) as OSCMessage;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.Address, Is.EqualTo(test_string));
            Assert.That(message.Arguments, Has.Length.EqualTo(1));
            Assert.That(message.Arguments[0], Is.EqualTo(new object[] { 1 }));
        }
    }

    [Test]
    public static void DecodingNestedArrayTest()
    {
        var msg = OSCDecoder.Decode(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x5B, 0x5B, 0x5B, 0x69, 0x5D, 0x5D, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }) as OSCMessage;
        Assert.That(msg, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(msg.Address, Is.EqualTo(test_string));
            Assert.That(msg.Arguments, Has.Length.EqualTo(1));
            Assert.That(msg.Arguments[0], Is.EqualTo(new object[] { new object[] { new object[] { 1 } } }));
        }
    }

    [Test]
    public static void DecodingBundleTest()
    {
        var message = OSCDecoder.Decode(new byte[] { 0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00, 0x83, 0xAA, 0x7E, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x2F, 0x74, 0x73, 0x32, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 }) as OSCBundle;

        Assert.That(message, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(message.TimeTag, Is.EqualTo(OSC.TimeTag(DateTime.UnixEpoch)));
            Assert.That(message.Packets, Has.Length.EqualTo(2));
            Assert.That(((OSCMessage)message.Packets[0]).Address, Is.EqualTo("/tst"));
            Assert.That(((OSCMessage)message.Packets[0]).Arguments[0], Is.EqualTo(1));
            Assert.That(((OSCMessage)message.Packets[1]).Address, Is.EqualTo("/ts2"));
            Assert.That(((OSCMessage)message.Packets[1]).Arguments[0], Is.EqualTo(2));
        }
    }
}