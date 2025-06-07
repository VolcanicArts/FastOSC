# FastOSC

FastOSC is a fast and memory-optimised C# OSC (Open Sound Control) library for .NET8+.

[![Nuget](https://img.shields.io/nuget/v/VolcanicArts.FastOSC)](https://www.nuget.org/packages/VolcanicArts.FastOSC/)

## Features

- **High Performance**: Designed for speed and low memory usage.
- **Ease of Use**: Simple and intuitive API.
- **Compatibility**: Supports the full [1.0 spec](https://opensoundcontrol.stanford.edu/spec-1_0.html). Extended support for UTF-8.

## Usage

```csharp
var sender = new OSCSender();
await sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9000));

var message = new OSCMessage("/address/test", new OSCMidi(1, 1, 1, 1));
sender.send(message);
```

```csharp
var receiver = new OSCReceiver();
receiver.OnPacketReceived += packet => // handle packet
receiver.Connect(new IPEndPoint(IPAddress.Loopback, 9001));
```

If you don't want to use the built-in sender and receiver:
```csharp
var message = new OSCMessage("/address/test", new OSCMidi(1, 1, 1, 1));
var data = OSCEncoder.Encode(message);
```

```csharp
var data = // some byte data

var packet = OSCDecoder.Decode(data);
if (packet is OSCMessage message)
{
    // handle message
}
else if (packet is OSCBundle bundle)
{
    // handle bundle
}
```

## Benchmarks
The benchmarks were ran with the address set to `/avatar/parameters/VRCOSC/Media/Position` and a single float argument, as it represents a real-world use of this library.
Benchmarks were ran on an i7-11700k, Windows 10.

|     Method      |     Mean      |   Error    |   StdDev   |       Op/s        |  Gen0   | Allocated |
|:---------------:|:-------------:|:----------:|:----------:|:-----------------:|:-------:|:---------:|
|  EncodeMessage  | **52.54 ns**  |  0.237 ns  |  0.198 ns  | **19,033,146.6**  | 0.0172  | **144 B** |
|  DecodeMessage  | **38.02 ns**  |  0.561 ns  |  0.525 ns  | **26,302,964.5**  | 0.0229  | **192 B** |