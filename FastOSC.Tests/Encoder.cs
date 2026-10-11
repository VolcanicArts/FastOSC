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

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x4E, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingInfinityTest()
    {
        var message = new OSCMessage(test_string, OSC.INFINITUM);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x49, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingBoolTrueTest()
    {
        var message = new OSCMessage(test_string, true);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x54, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingBoolFalseTest()
    {
        var message = new OSCMessage(test_string, false);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x46, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingIntTest()
    {
        var message = new OSCMessage(test_string, 1);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingFloatTest()
    {
        var message = new OSCMessage(test_string, 1f);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x66, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingFloat3Test()
    {
        var message = new OSCMessage(test_string, 1f, 1f, 1f);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x66, 0x66, 0x66, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingLongTest()
    {
        var message = new OSCMessage(test_string, 1L);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x68, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingDoubleTest()
    {
        var message = new OSCMessage(test_string, 1d);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x64, 0x00, 0x00, 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingStringTest()
    {
        var message = new OSCMessage(test_string, "some test string");
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x73, 0x6F, 0x6D, 0x65, 0x20, 0x74, 0x65, 0x73, 0x74, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x00, 0x00, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingBlobTest()
    {
        var message = new OSCMessage(test_string, new byte[] { 0x1, 0x2, 0x3, 0x4 });
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x01, 0x02, 0x03, 0x04 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingCharTest()
    {
        var message = new OSCMessage(test_string, 'a');
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x63, 0x00, 0x00, 0x61, 0x00, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingRGBATest()
    {
        var message = new OSCMessage(test_string, OSC.RGBA(1, 2, 3, 4));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x72, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingMIDITest()
    {
        var message = new OSCMessage(test_string, OSC.MIDI(1, OSCMIDIStatus.SystemExclusive, 3, 4));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x6D, 0x00, 0x00, 0x01, 0xF0, 0x03, 0x04 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingTimeTagTest()
    {
        var message = new OSCMessage(test_string, OSC.TimeTag(DateTime.UnixEpoch));
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x74, 0x00, 0x00, 0x83, 0xAA, 0x7E, 0x80, 0x00, 0x00, 0x00, 0x00 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingArrayTest()
    {
        var message = new OSCMessage(test_string, [new object[] { 1 }]);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x5B, 0x69, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingNestedArrayTest()
    {
        var message = new OSCMessage(test_string, [new object[] { new object[] { new object[] { 1 } } }]);
        var encodedData = OSCEncoder.Encode(message);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x5B, 0x5B, 0x5B, 0x69, 0x5D, 0x5D, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }));

        await sender.SendAsync(message);
    }

    [Test]
    public static async Task EncodingBundleTest()
    {
        var message1 = new OSCMessage("/tst", 1);
        var message2 = new OSCMessage("/ts2", 2);
        var bundle = new OSCBundle(DateTime.UnixEpoch, message1, message2);
        var encodedData = OSCEncoder.Encode(bundle);

        Assert.That(encodedData, Is.EqualTo(new byte[] { 0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00, 0x83, 0xAA, 0x7E, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x2F, 0x74, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x2F, 0x74, 0x73, 0x32, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 }));

        await sender.SendAsync(bundle);
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