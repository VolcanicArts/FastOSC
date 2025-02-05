// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public record OSCPacket
{
    private readonly OSCMessage? message;
    private readonly OSCBundle? bundle;

    public bool IsValid => bundle is not null || message is not null;
    public bool IsBundle => bundle is not null;

    public OSCPacket(OSCBundle? bundle)
    {
        this.bundle = bundle;
    }

    public OSCPacket(OSCMessage? message)
    {
        this.message = message;
    }

    public OSCMessage AsMessage() => message!;
    public OSCBundle AsBundle() => bundle!;
}