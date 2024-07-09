// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using BenchmarkDotNet.Attributes;

namespace FastOSC.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[OperationsPerSecond]
public class FastOSCBundle
{
    private OSCBundle bundle = null!;
    private byte[] encodedBaseline = null!;

    [GlobalSetup]
    public void Setup()
    {
        var message1 = new OSCMessage("/tst", 1);
        var message2 = new OSCMessage("/ts2", 2);
        var bundle2 = new OSCBundle(DateTime.Now, message1, message2);

        bundle = new OSCBundle(DateTime.Now, message1, bundle2, message2);
        encodedBaseline = OSCEncoder.Encode(bundle);
    }

    [Benchmark]
    public void Encode()
    {
        OSCEncoder.Encode(bundle);
    }

    [Benchmark]
    public void Decode()
    {
        OSCDecoder.Decode(encodedBaseline);
    }
}
