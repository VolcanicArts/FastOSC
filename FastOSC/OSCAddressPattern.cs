// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace FastOSC;

public record OSCAddressPattern
{
    public readonly string Pattern;
    private readonly Regex regex;

    public OSCAddressPattern(string pattern)
    {
        Pattern = pattern;
        regex = convertRootPatternToRegex(Pattern);
    }

    public bool IsMatch(string matchingAddress) => regex.IsMatch(matchingAddress);

    public static bool IsValid(string pattern)
    {
        try
        {
            convertRootPatternToRegex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Regex convertRootPatternToRegex(string pattern)
    {
        var fullRegex = $"^{convertPatternToRegex(pattern)}$";
        return new Regex(fullRegex);
    }

    private static string convertPatternToRegex(string pattern)
    {
        var regex = "";
        var i = 0;

        while (i < pattern.Length)
        {
            var c = pattern[i];

            switch (c)
            {
                case '?':
                    regex += ".";
                    break;

                case '*':
                    regex += ".*";
                    break;

                case '[':
                    var j = i + 1;
                    var foundClosingBracket = false;

                    while (j < pattern.Length)
                    {
                        if (pattern[j] == ']')
                        {
                            foundClosingBracket = true;
                            break;
                        }

                        j++;
                    }

                    if (!foundClosingBracket)
                        throw new ArgumentException("Unmatched '[' in OSC pattern");

                    var bracketContent = pattern.Substring(i + 1, j - i - 1);

                    if (bracketContent.StartsWith('!'))
                        bracketContent = $"^{bracketContent[1..]}";

                    regex += $"[{bracketContent}]";
                    i = j;
                    break;

                case ']':
                    throw new ArgumentException("Unmatched ']' in OSC pattern");

                case '{':
                    var start = i + 1;
                    var end = start;
                    var depth = 1;

                    while (end < pattern.Length && depth > 0)
                    {
                        switch (pattern[end])
                        {
                            case '{':
                                depth++;
                                break;

                            case '}':
                                depth--;
                                break;
                        }

                        end++;
                    }

                    if (depth != 0)
                        throw new ArgumentException("Unmatched '{' in OSC pattern");

                    var inner = pattern.Substring(start, end - start - 1);
                    var parts = inner.Split(',');
                    var subPatterns = parts.Select(convertPatternToRegex);

                    regex += $"({string.Join("|", subPatterns)})";
                    i = end - 1;
                    break;

                case '}':
                    throw new ArgumentException("Unmatched '}' in OSC pattern");

                case '.':
                case '+':
                case '(':
                case ')':
                case '^':
                case '$':
                case '|':
                case '\\':
                    regex += $"\\{c}";
                    break;

                default:
                    regex += c;
                    break;
            }

            i++;
        }

        return regex;
    }
}