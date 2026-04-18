// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers;
using BenchmarkDotNet.Attributes;
using Rug.Osc;

namespace FastOSC.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
public class FastOSC
{
    private const string test_address = "/0/dmx/0";
    private const float test_value = 0.5f;
    private const string test_pattern = "/foo*bar[abc]?";
    private const string test_pattern_address = "/foobarac";

    private OSCMessage message = null!;
    private byte[] encodedMessage = null!;
    private readonly ArrayPool<byte> pool = ArrayPool<byte>.Shared;
    private byte[] rentedArray = null!;
    private OscMessage rugOscMessage = null!;

    [GlobalSetup]
    public void Setup()
    {
        message = new OSCMessage(test_address, test_value);
        encodedMessage = OSCEncoder.Encode(message);
        rentedArray = pool.Rent(128);
        rugOscMessage = new OscMessage(test_address, test_value);
    }

    [Benchmark]
    public void FastOSC_FullMessageEncode()
    {
        var localMessage = new OSCMessage(test_address, 1f, 1f, 1f, OSC.RGBA(255, 0, 0, 255));
        var data = OSCEncoder.Encode(localMessage);
    }

    [Benchmark(Baseline = true)]
    public void RugOsc_FullMessageEncode()
    {
        var localMessage = new OscMessage(test_address, 1f, 1f, 1f, OscColor.FromArgb(255, 255, 0, 0));
        var data = localMessage.ToByteArray();
    }

    //[Benchmark(Baseline = true)]
    public void FastOSC_MessageEncode()
    {
        OSCEncoder.Encode(message);
    }

    //[Benchmark]
    public void RugOsc_MessageEncode()
    {
        rugOscMessage.ToByteArray();
    }

    //[Benchmark(Baseline = true)]
    public void FastOSC_MessageEncode_ArrayPool()
    {
        OSCEncoder.Encode(message, rentedArray);
    }

    //[Benchmark]
    public void RugOsc_MessageEncode_ArrayPool()
    {
        rugOscMessage.Write(rentedArray);
    }

    //[Benchmark(Baseline = true)]
    public void FastOSC_MessageDecode()
    {
        OSCDecoder.Decode(encodedMessage);
    }

    //[Benchmark]
    public void RugOsc_MessageDecode()
    {
        OscMessage.Read(encodedMessage, encodedMessage.Length);
    }

    //[Benchmark(Baseline = true)]
    public void FastOSC_PatternMatching()
    {
        var pattern = new OSCAddressPattern(test_pattern);
        pattern.IsMatch(test_pattern_address);
    }

    //[Benchmark]
    public void RugOsc_PatternMatching()
    {
        var pattern = new OscAddress(test_pattern);
        pattern.Match(test_pattern_address);
    }

    //[Benchmark(Baseline = true)]
    public void FastOSC_Bundle()
    {
        var message1 = new OSCMessage("/tst", 1);
        var message2 = new OSCMessage("/tst1", 2f);
        var message3 = new OSCMessage("/tst11", OSC.MIDI(64, OSCMIDIStatus.SystemExclusive, 192, 255));
        var bundleInner = new OSCBundle(OSC.EPOCH, message2, message3);
        var bundle = new OSCBundle(OSC.EPOCH, message1, bundleInner);
        OSCEncoder.Encode(bundle);
    }

    //[Benchmark]
    public void RugOsc_Bundle()
    {
        var message1 = new OscMessage("/tst", 1);
        var message2 = new OscMessage("/tst1", 2f);
        var message3 = new OscMessage("/tst11", new OscMidiMessage(64, OscMidiSystemMessageType.SystemExclusive, 192, 255));
        var bundleInner = new OscBundle(OSC.EPOCH, message2, message3);
        var bundle = new OscBundle(OSC.EPOCH, message1, bundleInner);
        bundle.ToByteArray();
    }
}