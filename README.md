# FastOSC

FastOSC is a fast and memory-optimised C# OSC (Open Sound Control) library for .NET 8 and above.

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

Full OSC 1.0 spec is supported for address pattern matching.

```csharp
var pattern = new OSCAddressPattern("/{test?,data*,[a-z]oo}");
var isMatch = pattern.IsMatch("/test1");
```

## Benchmarks
Encode/Decode benchmarks were ran with the address set to `/avatar/parameters/VRCOSC/Media/Position` and a single float argument, as it represents a real-world use of this library.
Benchmarks were ran on an i7-11700k, Windows 10.

| Operation        | Library             | Mean         | Speed Comparison | Ops/sec | Mem Alloc | Memory Comparison |
| ---------------- |---------------------|--------------|------------------| ------- | --------- |-------------------|
| Message Encode   | FastOSC             | 36.57 ns     | 2.27x faster     | 27.34 M | 80 B      | 2.1x less         |
|                  | FastOSC (ArrayPool) | 19.39 ns     | 4.28x faster     | 51.58 M | 0 B       | Inf x less        |
|                  | RugOsc              | 83.11 ns     | –                | 12.03 M | 168 B     | –                 |
| Message Decode   | FastOSC             | 35.95 ns     | 3.75x faster     | 27.81 M | 192 B     | 2.5x less         |
|                  | RugOsc              | 134.77 ns    | –                | 7.42 M  | 480 B     | –                 |
| Pattern Matching | FastOSC             | 1,165.00 ns  | 2.81x faster     | 0.86 M  | 3,270 B   | 3.2x less         |
|                  | RugOsc              | 3,274.29 ns  | –                | 0.31 M  | 10,528 B  | –                 |