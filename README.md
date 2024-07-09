# FastOSC

FastOSC is a fast and memory-optimised C# OSC (Open Sound Control) library for .Net 6.

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

var packet = OSCDecoder.Decode(buffer);
if (!packet.IsValid) return;

if (packet.IsBundle)
    // handle bundle
else
    // handle message
```

## Benchmarks
Encoding and decoding a single OSCMidi value.
| Method | Job        | BuildConfiguration | Mean     | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|------- |----------- |------------------- |---------:|---------:|---------:|-------------:|-------:|----------:|
| Encode | DefaultJob | Default            | 66.38 ns | 1.347 ns | 1.551 ns | 15,064,602.9 | 0.0105 |      88 B |
| Decode | DefaultJob | Default            | 50.86 ns | 1.023 ns | 1.681 ns | 19,662,228.0 | 0.0200 |     168 B |

Encoding and decoding 3 floats.
| Method | Job        | BuildConfiguration | Mean     | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|------- |----------- |------------------- |---------:|---------:|---------:|-------------:|-------:|----------:|
| Encode | DefaultJob | Default            | 72.27 ns | 1.467 ns | 1.802 ns | 13,836,737.2 | 0.0124 |     104 B |
| Decode | DefaultJob | Default            | 57.13 ns | 1.155 ns | 2.280 ns | 17,503,529.9 | 0.0277 |     232 B |

