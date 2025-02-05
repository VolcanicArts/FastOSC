// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public record OSCBundle : IOSCElement
{
    public readonly OSCTimeTag TimeTag;
    public readonly IOSCElement[] Elements;

    public OSCBundle(DateTime dateTime, params IOSCElement[] elements)
    {
        TimeTag = new OSCTimeTag(dateTime);
        Elements = elements;
    }

    public OSCBundle(OSCTimeTag timeTag, params IOSCElement[] elements)
    {
        TimeTag = timeTag;
        Elements = elements;
    }
}