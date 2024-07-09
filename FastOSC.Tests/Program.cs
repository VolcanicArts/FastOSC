// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;

namespace FastOSC.Tests;

public class Program
{
    public static void Playground()
    {
        var message1 = new OSCMessage("/tst", 1);
        var message2 = new OSCMessage("/ts2", 2);
        var bundle2 = new OSCBundle(DateTime.Now, message1, message2);

        var bundle = new OSCBundle(DateTime.Now, message1, bundle2, message2);
        var data = OSCEncoder.Encode(bundle);

        OSCUtils.PrintByteArray(data);

        Console.WriteLine();

        var element = OSCDecoder.Decode(data)!;
        displayBundle(element.AsBundle());
    }

    private static void displayBundle(OSCBundle bundle)
    {
        Console.WriteLine(bundle.TimeTag.AsDateTime().ToString(CultureInfo.CurrentCulture));

        foreach (var nestedElement in bundle.Elements)
        {
            if (nestedElement is OSCBundle elementBundle)
            {
                displayBundle(elementBundle);
            }
            else if (nestedElement is OSCMessage elementMessage)
            {
                Console.WriteLine(elementMessage.Address + " - " + string.Join(", ", elementMessage.Arguments.Select(argument => argument?.ToString())));
            }
        }
    }
}
