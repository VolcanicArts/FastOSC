// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC;

public static class OSCConst
{
    public static readonly DateTime OSC_EPOCH = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public const byte INT = 105; // 'i'
    public const byte FLOAT = 102; // 'f'
    public const byte STRING = 115; // 's'
    public const byte BLOB = 98; // 'b'
    public const byte LONG = 104; // 'h'
    public const byte TIMETAG = 116; // 't'
    public const byte DOUBLE = 100; // 'd'
    public const byte ALT_STRING = 83; // 'S'
    public const byte CHAR = 99; // 'c'
    public const byte RGBA = 114; // 'r'
    public const byte MIDI = 109; // 'm'
    public const byte TRUE = 84; // 'T'
    public const byte FALSE = 70; // 'F'
    public const byte NIL = 78; // 'N'
    public const byte INFINITY = 73; // 'I'
    public const byte ARRAY_BEGIN = 91; // '['
    public const byte ARRAY_END = 93; // ']'
    public const byte COMMA = 44; // ','
    public const byte SLASH = 47; // '/'
}