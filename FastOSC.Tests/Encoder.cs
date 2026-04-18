// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Net;

namespace FastOSC.Tests;

public static class Encoder
{
    private const string test_string = "/tst";

    private static OSCSender sender = null!;

    [OneTimeSetUp]
    public static async Task Setup()
    {
        sender = new OSCSender();
        await sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9000));
    }

    [OneTimeTearDown]
    public static void TearDown()
    {
        sender.Disconnect();
    }

    [Test]
    public static async Task EncodingNilTest()
    {
        var message = new OSCMessage(test_string, OSC.NIL);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,N\0\0"u8.ToArray()));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingInfinityTest()
    {
        var message = new OSCMessage(test_string, OSC.INFINITUM);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,I\0\0"u8.ToArray()));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingBoolTrueTest()
    {
        var message = new OSCMessage(test_string, true);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,T\0\0"u8.ToArray()));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingBoolFalseTest()
    {
        var message = new OSCMessage(test_string, false);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,F\0\0"u8.ToArray()));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingIntTest()
    {
        var message = new OSCMessage(test_string, 1);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,i\0\0"u8.ToArray().Concat(new byte[] { 0x00, 0x00, 0x00, 0x01 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingFloatTest()
    {
        var message = new OSCMessage(test_string, 1f);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,f\0\0"u8.ToArray().Concat(new byte[] { 0x3F, 0x80, 0x00, 0x00 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingFloat3Test()
    {
        var message = new OSCMessage(test_string, 1f, 1f, 1f);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,fff\0\0\0\0"u8.ToArray().Concat(new byte[] { 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingLongTest()
    {
        var message = new OSCMessage(test_string, 1L);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,h\0\0"u8.ToArray().Concat(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingDoubleTest()
    {
        var message = new OSCMessage(test_string, 1d);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,d\0\0"u8.ToArray().Concat(new byte[] { 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingStringTest()
    {
        var message = new OSCMessage(test_string, "some test string");
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,s\0\0some test string\0\0\0\0"u8.ToArray()));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingBlobTest()
    {
        var message = new OSCMessage(test_string, new byte[] { 0x1, 0x2, 0x3, 0x4 });
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,b\0\0"u8.ToArray().Concat(new byte[] { 0x00, 0x00, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingCharTest()
    {
        var message = new OSCMessage(test_string, 'a');
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,c\0\0\0\0\0a"u8.ToArray()));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingRGBATest()
    {
        var message = new OSCMessage(test_string, OSC.RGBA(1, 2, 3, 4));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,r\0\0"u8.ToArray().Concat(new byte[] { 0x01, 0x02, 0x03, 0x04 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingMIDITest()
    {
        var message = new OSCMessage(test_string, OSC.MIDI(1, OSCMIDIStatus.SystemExclusive, 3, 4));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,m\0\0"u8.ToArray().Concat(new byte[] { 0x01, 0xF0, 0x03, 0x04 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingTimeTagTest()
    {
        var message = new OSCMessage(test_string, OSC.TimeTag(1234ul));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,t\0\0"u8.ToArray().Concat(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0xD2 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingArrayTest()
    {
        var message = new OSCMessage(test_string, [new object[] { 1 }]);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,[i]"u8.ToArray().Concat(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingNestedArrayTest()
    {
        var message = new OSCMessage(test_string, [new object[] { new object[] { new object[] { 1 } } }]);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo("/tst\0\0\0\0,[[[i]]]"u8.ToArray().Concat(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 })));

        await sender.Send(message);
    }

    [Test]
    public static async Task EncodingBundleTest()
    {
        var message1 = new OSCMessage("/tst", 1);
        var message2 = new OSCMessage("/ts2", 2);
        var bundle = new OSCBundle(OSC.EPOCH, message1, message2);
        var encodedData = OSCEncoder.Encode(bundle);

        Assert.That(encodedData, Is.EqualTo("#bundle\0\0\0\0\0\0\0\0\0\0\0\0\u0010/tst\0\0\0\0,i\0\0\0\0\0\u0001\0\0\0\u0010/ts2\0\0\0\0,i\0\0\0\0\0\u0002"u8.ToArray()));

        await sender.Send(bundle);
    }

    [Test]
    public static void AddressAlignBoundaries()
    {
        assertMessageStartsCorrectly("/a", 4);
        assertMessageStartsCorrectly("/ab", 4);
        assertMessageStartsCorrectly("/abc", 8);
        assertMessageStartsCorrectly("/abcd", 8);
        assertMessageStartsCorrectly("/abcdef", 8);
        assertMessageStartsCorrectly("/abcdefg", 12);
        assertMessageStartsCorrectly("/abcdefgh", 12);
    }

    private static void assertMessageStartsCorrectly(string address, int expectedTypeTagOffset)
    {
        var message = new OSCMessage(address, true);
        var data = OSCEncoder.Encode(message);
        Assert.That((char)data[expectedTypeTagOffset], Is.EqualTo(','));
    }
}