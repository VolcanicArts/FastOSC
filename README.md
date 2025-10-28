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

var message = new OSCMessage("/address/test", 1f);
sender.send(message);
```

```csharp
var receiver = new OSCReceiver();
receiver.OnPacketReceived += packet => // handle packet
receiver.Connect(new IPEndPoint(IPAddress.Loopback, 9001));
```

If you don't want to use the built-in sender and receiver:
```csharp
var message = new OSCMessage("/address/test", 1f);
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

On average, FastOSC runs 3x faster and uses 3x less memory than RugOsc.

| Operation              | Library             | Mean         | Speed Comparison | Ops/sec | Mem Alloc | Memory Comparison |
| ----------------------- | ------------------- | ------------ | ---------------- | ------- | --------- |-------------------|
| Message Encode          | FastOSC             | 37.01 ns     | 2.57× faster     | 27.02 M | 80 B      | 3.1× less         |
|                         | RugOsc              | 95.14 ns     | –                | 10.51 M | 248 B     | –                 |
| Message Encode (Pool)   | FastOSC             | 20.27 ns     | 3.60× faster     | 49.32 M | 0 B       | Inf× less         |
|                         | RugOsc              | 72.96 ns     | –                | 13.71 M | 168 B     | –                 |
| Message Decode          | FastOSC             | 36.40 ns     | 3.44× faster     | 27.48 M | 192 B     | 2.5× less         |
|                         | RugOsc              | 125.01 ns    | –                | 8.00 M  | 480 B     | –                 |
| Pattern Matching        | FastOSC             | 1,142.00 ns  | 2.68× faster     | 0.88 M  | 3,270 B   | 3.15× less        |
|                         | RugOsc              | 3,065.00 ns  | –                | 0.33 M  | 10,280 B  | –                 |
| Bundle                  | FastOSC             | 276.60 ns    | 3.25× faster     | 3.62 M  | 528 B     | 3.2× less         |
|                         | RugOsc              | 898.20 ns    | –                | 1.11 M  | 1,688 B   | –                 |