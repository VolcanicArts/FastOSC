// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Buffers;
using BenchmarkDotNet.Attributes;
using Rug.Osc;

namespace FastOSC.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[OperationsPerSecond]
public class FastOSC
{
    private const string test_address = "/avatar/parameters/VRCOSC/Media/Position";
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
    public void FastOSC_MessageEncode()
    {
        OSCEncoder.Encode(message);
    }

    [Benchmark]
    public void FastOSC_MessageEncode_DestArray()
    {
        OSCEncoder.Encode(message, rentedArray);
    }

    [Benchmark]
    public void FastOSC_MessageDecode()
    {
        OSCDecoder.Decode(encodedMessage);
    }

    [Benchmark]
    public void FastOSC_PatternMatching()
    {
        var pattern = new OSCAddressPattern(test_pattern);
        pattern.IsMatch(test_pattern_address);
    }

    [Benchmark]
    public void RugOsc_MessageEncode()
    {
        rugOscMessage.Write(rentedArray);
    }

    [Benchmark]
    public void RugOsc_MessageDecode()
    {
        OscMessage.Read(encodedMessage, encodedMessage.Length);
    }

    [Benchmark]
    public void RugOsc_PatternMatching()
    {
        var pattern = new OscAddress(test_pattern);
        pattern.Match(test_pattern_address);
    }
}