# FastOSC

FastOSC is a fast and memory-optimised C# OSC (Open Sound Control) library for .NET 6.

![NuGet Version](https://img.shields.io/nuget/v/VolcanicArts.FastOSC)

## Features

- **High Performance**: Designed for speed and low memory usage.
- **Ease of Use**: Simple and intuitive API.
- **Compatibility**: Supports the full [1.0 spec](https://opensoundcontrol.stanford.edu/spec-1_0.html). Extended support for all .Net encoding types including ASCII, UTF-8, and Unicode.

## Fully Supported Types
- int32 (Int32)
- int64 (Int64 / Long)
- float32 (Single)
- float64 (Double)
- String (String / Symbol)
- Blob (Byte[])
- TimeTag (UInt64 / ULong / DateTime)
- Single characters (Char)
- 32 bit RGBA color (OSCRGBA)
- 4 byte MIDI message (OSCMidi)
- True (Boolean)
- False (Boolean)
- Nil (null)
- Infinitum (Single.PositiveInfinity)
- Nested arrays (Object?[])
- Bundles (OSCBundle)

## Usage

```c#
var sender = new OSCSender();
await sender.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9000));

var message = new OSCMessage("/address/test", new OSCMidi(1, 1, 1, 1));
sender.send(message);
```

```c#
var receiver = new OSCReceiver();
receiver.Enable(new IPEndPoint(IPAddress.Loopback, 9001));
receiver.OnMessageReceived += message => { };
receiver.OnBundleReceived += bundle => { };
```

If you don't want to use the built-in sender and receiver:
```c#
var message = new OSCMessage("/address/test", new OSCMidi(1, 1, 1, 1));
var data = OSCEncoder.Encode(message);
```

```c#
var data = new byte[];

var packet = OSCDecoder.Decode(data);
if (!packet.IsValid) return;

if (packet.IsBundle)
    // handle bundle
else
    // handle message
```

## Benchmarks
All benchmarks were ran with the address `/address/test`.

Encoding and decoding an OSCMidi value:

| Method | Mean     | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|------- |---------:|---------:|---------:|-------------:|-------:|----------:|
| Encode | 53.82 ns | 1.103 ns | 3.020 ns | 18,580,512.0 | 0.0105 |      88 B |
| Decode | 42.27 ns | 0.851 ns | 1.599 ns | 23,657,956.0 | 0.0200 |     168 B |

Encoding and decoding 3 floats values:

| Method | Mean     | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|------- |---------:|---------:|---------:|-------------:|-------:|----------:|
| Encode | 59.40 ns | 1.172 ns | 1.523 ns | 16,835,787.8 | 0.0124 |     104 B |
| Decode | 51.94 ns | 1.051 ns | 1.330 ns | 19,251,212.6 | 0.0277 |     232 B |
