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
Encode/Decode benchmarks were ran with the address set to `/avatar/parameters/VRCOSC/Media/Position` and a single float argument, as it represents a real-world use of this library.
Benchmarks were ran on an i7-11700k, Windows 10.

| Operation            | Library             | Mean (ns)  | StdDev (ns) | Ops/sec      | Allocated (B) | Gen0   | Gen1   |
|----------------------|---------------------|------------|-------------|--------------|---------------| ------ | ------ |
| **Message Encode**   | FastOSC             | **35.75**  | 0.375       | **~27.97 M** | **80**        | 0.0095 | -      |
|                      | FastOSC (ArrayPool) | **24.14**  | 0.146       | **~41.43 M** | **0**         | -      | -      |
|                      | RugOsc              | 83.11      | 2.129       | ~12.03 M     | 168           | 0.0200 | -      |
| **Message Decode**   | FastOSC             | **39.00**  | 0.722       | **~25.64 M** | **192**       | 0.0229 | -      |
|                      | RugOsc              | 134.77     | 1.260       | ~7.42 M      | 480           | 0.0572 | -      |
| **Pattern Matching** | FastOSC             | 1,297.00   | 28.000      | ~0.77 M      | 3,270         | 0.3986 | 0.0019 |
|                      | FastOSC (Caching)   | **107.54** | 0.452       | **~9.30 M**  | **32**        | 0.0038 | -      |
|                      | RugOsc              | 3,274.29   | 86.136      | ~0.31 M      | 10,528        | 1.2550 | 0.0381 |