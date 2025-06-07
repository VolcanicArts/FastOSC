// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using BenchmarkDotNet.Attributes;

namespace FastOSC.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[OperationsPerSecond]
public class FastOSC
{
    private OSCMessage message = null!;
    private byte[] encodedMessage = null!;
    private OSCBundle bundle = null!;
    private byte[] encodedBundle = null!;

    [GlobalSetup]
    public void Setup()
    {
        message = new OSCMessage("/avatar/parameters/VRCOSC/Media/Position", 0.5f);
        encodedMessage = OSCEncoder.Encode(message);
        bundle = new OSCBundle(new OSCTimeTag(DateTime.Now), message, message, message, message, message, message, message, message, message, message, message, message, message, message, message);
        encodedBundle = OSCEncoder.Encode(bundle);
    }

    [Benchmark]
    public void EncodeMessage()
    {
        OSCEncoder.Encode(message);
    }

    [Benchmark]
    public void EncodeBundle()
    {
        OSCEncoder.Encode(bundle);
    }

    [Benchmark]
    public void DecodeMessage()
    {
        OSCDecoder.Decode(encodedMessage);
    }

    [Benchmark]
    public void DecodeBundle()
    {
        OSCDecoder.Decode(encodedBundle);
    }
}