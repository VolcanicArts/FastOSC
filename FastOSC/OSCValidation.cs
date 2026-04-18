// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;

namespace FastOSC;

public static class OSCValidation
{
    public static void ThrowIfInvalidAddress(string address, [CallerArgumentExpression(nameof(address))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address, paramName);

        if (address.Length < 2 || address[0] != '/')
            throw new ArgumentException("Address must start with '/' and have at least one character after it", paramName);
    }
}