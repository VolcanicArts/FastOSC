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
    private byte[] encodedBaseline = null!;

    [GlobalSetup]
    public void Setup()
    {
        message = new OSCMessage("/address/test", 1.0f, 0.5f, 0.0f);
        encodedBaseline = OSCEncoder.Encode(message);
    }

    [Benchmark]
    public void Encode()
    {
        OSCEncoder.Encode(message);
    }

    [Benchmark]
    public void Decode()
    {
        OSCDecoder.Decode(encodedBaseline);
    }
}
