// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public class OSCMessage
{
    public string Address { get; }
    public object?[] Arguments { get; }

    public OSCMessage(string address, params object?[] arguments)
    {
        if (address.Length == 0) throw new InvalidOperationException($"{nameof(address)} must have a non-zero length");
        if (arguments.Length == 0) throw new InvalidOperationException($"{nameof(arguments)} must have a non-zero length");

        Address = address;
        Arguments = arguments;
    }
}
