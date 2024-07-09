# FastOSC

FastOSC is the fastest and most memory-efficient C# OSC (Open Sound Control) library. It is designed to be high-performance and low-memory.

## Features

- **High Performance**: Optimised for speed and low memory usage.
- **Ease of Use**: Simple and intuitive API.
- **Compatibility**: Supports the full [1.0 spec](https://opensoundcontrol.stanford.edu/spec-1_0.html). Extended support for ASCII, UTF-8, and Unicode.

## Supported Types
- int32 (Int32)
- int64 (Int64 / Long)
- float32 (Single)
- float64 (Double)
- String (String / Symbol)
- Blob (Byte[])
- TimeTag (UInt64 / DateTime / TimeSpan)
- Single characters (Char)
- 32 bit RGBA color (OSCRGBA)
- 4 byte MIDI message (OSCMidi)
- True (Boolean)
- False (Boolean)
- Nil (null)
- Infinitum (Single.PositiveInfinity)
- Nested arrays (Object?[])

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
```

## Benchmarks
Encoding and decoding a single OSCMidi value.
| Method | Job        | BuildConfiguration | Mean     | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|------- |----------- |------------------- |---------:|---------:|---------:|-------------:|-------:|----------:|
| Encode | DefaultJob | Default            | 55.30 ns | 1.080 ns | 1.286 ns | 18,082,410.1 | 0.0105 |      88 B |
| Decode | DefaultJob | Default            | 31.19 ns | 0.612 ns | 1.040 ns | 32,065,285.3 | 0.0162 |     136 B |

Encoding and decoding 3 floats.
| Method | Job        | BuildConfiguration | Mean     | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|------- |----------- |------------------- |---------:|---------:|---------:|-------------:|-------:|----------:|
| Encode | DefaultJob | Default            | 55.02 ns | 0.863 ns | 0.807 ns | 18,175,310.6 | 0.0124 |     104 B |
| Decode | DefaultJob | Default            | 38.93 ns | 0.622 ns | 0.520 ns | 25,690,239.6 | 0.0239 |     200 B |
