// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Globalization;
using System.Net;

namespace FastOSC.Tests;

public class Program
{
    public static async void Playground()
    {
        var receiver = new OSCReceiver();
        receiver.OnMessageReceived += m => Console.WriteLine($"Received: {m.Address} {m.Arguments[0]}");
        receiver.Connect(new IPEndPoint(IPAddress.Loopback, 9001));

        var sender = new OSCSender();
        await sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9000));
        sender.Send(new OSCMessage("/test", false));

        Console.ReadKey();

        await receiver.DisconnectAsync();
        sender.Disconnect();
    }

    private static void displayBundle(OSCBundle bundle)
    {
        Console.WriteLine(bundle.TimeTag.AsDateTime().ToString(CultureInfo.CurrentCulture));

        foreach (var nestedElement in bundle.Elements)
        {
            switch (nestedElement)
            {
                case OSCBundle elementBundle:
                    displayBundle(elementBundle);
                    break;

                case OSCMessage elementMessage:
                    Console.WriteLine(elementMessage.Address + " - " + string.Join(", ", elementMessage.Arguments.Select(argument => argument?.ToString())));
                    break;
            }
        }
    }
}