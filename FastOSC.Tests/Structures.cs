// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public static class Structures
{
    [Test]
    public static void Construct_OSCMessage()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.DoesNotThrow(() => _ = new OSCMessage("/test", 1));
            Assert.Throws<ArgumentException>(() => _ = new OSCMessage("/", 1));
            Assert.Throws<ArgumentException>(() => _ = new OSCMessage("a", 1));
            Assert.Throws<ArgumentException>(() => _ = new OSCMessage(string.Empty, 1));
            Assert.Throws<ArgumentNullException>(() => _ = new OSCMessage(null!, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new OSCMessage("/test"));
        }
    }

    [Test]
    public static void Construct_OSCBundle()
    {
        using (Assert.EnterMultipleScope())
        {
            var validMessage = new OSCMessage("/test", 1);

            Assert.DoesNotThrow(() => _ = new OSCBundle(OSC.EPOCH, validMessage));
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new OSCBundle(OSC.EPOCH));
        }
    }
}