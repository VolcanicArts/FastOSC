// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace FastOSC.Benchmarks;

public class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<FastOSC>(new DebugBuildConfig());
    }
}
