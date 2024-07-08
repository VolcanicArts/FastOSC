// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;

namespace FastOSC;

public static class OSCUtils
{
    /// <summary>
    /// Aligns an index to an interval of 4
    /// </summary>
    /// <remarks>This will force add 4 even if the index is aligned to facilitate the spec's required gap</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Align(int index) => index + (4 - index % 4);
}
