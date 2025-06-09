// Copyright (c) VolcanicArts. Licensed under the LGPL License.
// See the LICENSE file in the repository root for full license text.

namespace FastOSC.Tests;

public class AddressPatterns
{
    [Test]
    public void MatchWildcardAsteriskZeroCharacters()
    {
        var pattern = new OSCAddressPattern("/test*");
        Assert.That(pattern.IsMatch("/test"));
    }

    [Test]
    public void MatchWildcardAsteriskManyCharacters()
    {
        var pattern = new OSCAddressPattern("/test*");
        Assert.That(pattern.IsMatch("/test_abc"));
    }

    [Test]
    public void MatchWildcardQuestionMark()
    {
        var pattern = new OSCAddressPattern("/te?t");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/test"));
            Assert.That(!pattern.IsMatch("/teest"));
        }
    }

    [Test]
    public void MatchCharacterClassAnyOf()
    {
        var pattern = new OSCAddressPattern("/file[abc]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/filea"));
            Assert.That(pattern.IsMatch("/fileb"));
            Assert.That(pattern.IsMatch("/filec"));
            Assert.That(!pattern.IsMatch("/filed"));
        }
    }

    [Test]
    public void MatchCharacterClassRange()
    {
        var pattern = new OSCAddressPattern("/file[a-c]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/filea"));
            Assert.That(pattern.IsMatch("/fileb"));
            Assert.That(pattern.IsMatch("/filec"));
            Assert.That(!pattern.IsMatch("/filed"));
        }
    }

    [Test]
    public void MatchCharacterClassNegation()
    {
        var pattern = new OSCAddressPattern("/file[!x-z]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/filea"));
            Assert.That(!pattern.IsMatch("/filex"));
        }
    }

    [Test]
    public void MatchCharacterClassWithDashAtEnd()
    {
        var pattern = new OSCAddressPattern("/file[abc-]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/file-"));
            Assert.That(pattern.IsMatch("/fileb"));
            Assert.That(!pattern.IsMatch("/filex"));
        }
    }

    [Test]
    public void MatchCharacterClassWithExclamationNotAtStart()
    {
        var pattern = new OSCAddressPattern("/file[a!b]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/file!"));
            Assert.That(pattern.IsMatch("/filea"));
            Assert.That(pattern.IsMatch("/fileb"));
            Assert.That(!pattern.IsMatch("/filec"));
        }
    }

    [Test]
    public void MatchAlternatives()
    {
        var pattern = new OSCAddressPattern("/{foo,bar,baz}");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/foo"));
            Assert.That(pattern.IsMatch("/bar"));
            Assert.That(pattern.IsMatch("/baz"));
            Assert.That(!pattern.IsMatch("/qux"));
        }
    }

    [Test]
    public void MatchAlternativesWithWildcard()
    {
        var pattern = new OSCAddressPattern("/{foo,bar}*");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/foobar"));
            Assert.That(pattern.IsMatch("/bar123"));
            Assert.That(!pattern.IsMatch("/baz123"));
        }
    }

    [Test]
    public void MatchEscapedRegexCharacters()
    {
        var pattern = new OSCAddressPattern("/file.abc");
        Assert.That(!pattern.IsMatch("/file1abc"));
    }

    [Test]
    public void MatchWildcardInsideAlternatives()
    {
        var pattern = new OSCAddressPattern("/{foo*,bar?}");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/foobar"));
            Assert.That(pattern.IsMatch("/bar1"));
            Assert.That(!pattern.IsMatch("/bar12"));
        }
    }

    [Test]
    public void Error_UnmatchedBracket()
    {
        var ex = Assert.Throws<ArgumentException>(() => _ = new OSCAddressPattern("/test[abc"));
        Assert.That(ex.Message, Does.Contain("Unmatched '['"));
    }

    [Test]
    public void Error_UnmatchedClosingBracket()
    {
        var ex = Assert.Throws<ArgumentException>(() => _ = new OSCAddressPattern("/test]"));
        Assert.That(ex.Message, Does.Contain("Unmatched ']'"));
    }

    [Test]
    public void Error_UnmatchedBrace()
    {
        var ex = Assert.Throws<ArgumentException>(() => _ = new OSCAddressPattern("/test{foo,bar"));
        Assert.That(ex.Message, Does.Contain("Unmatched '{'"));
    }

    [Test]
    public void Error_UnmatchedClosingBrace()
    {
        var ex = Assert.Throws<ArgumentException>(() => _ = new OSCAddressPattern("/test}"));
        Assert.That(ex.Message, Does.Contain("Unmatched '}'"));
    }

    [Test]
    public void MatchMultipleAsterisks()
    {
        var pattern = new OSCAddressPattern("/foo*bar*baz");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/foobarbaz"));
            Assert.That(pattern.IsMatch("/fooXbarYbaz"));
            Assert.That(pattern.IsMatch("/foobar_YZbaz"));
            Assert.That(!pattern.IsMatch("/foobaz"));
        }
    }

    [Test]
    public void MatchMixedWildcardAndCharClass()
    {
        var pattern = new OSCAddressPattern("/foo*bar[abc]?");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/foobarac"));
            Assert.That(pattern.IsMatch("/foo123barbZ"));
            Assert.That(!pattern.IsMatch("/foo123bardZ"));
            Assert.That(!pattern.IsMatch("/foobar"));
        }
    }

    [Test]
    public void MatchWildcardFollowedByAlternatives()
    {
        var pattern = new OSCAddressPattern("/*/{x,y,z}");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/anything/x"));
            Assert.That(pattern.IsMatch("/123/y"));
            Assert.That(pattern.IsMatch("/abc/z"));
            Assert.That(!pattern.IsMatch("/abc/w"));
        }
    }

    [Test]
    public void MatchCharacterClassRangeAndNegation()
    {
        var pattern = new OSCAddressPattern("/data/[!a-cx-z]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/data/d"));
            Assert.That(pattern.IsMatch("/data/m"));
            Assert.That(!pattern.IsMatch("/data/a"));
            Assert.That(!pattern.IsMatch("/data/z"));
        }
    }

    [Test]
    public void MatchNestedWildcardsWithAlternatives()
    {
        var pattern = new OSCAddressPattern("/{foo*,bar[1-9]*,baz?}/end");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/foobar/end"));
            Assert.That(pattern.IsMatch("/bar5stuff/end"));
            Assert.That(pattern.IsMatch("/baz1/end"));
            Assert.That(!pattern.IsMatch("/bar0stuff/end"));
            Assert.That(!pattern.IsMatch("/baz12/end"));
        }
    }

    [Test]
    public void MatchAlternativesWithMixedWildcardsAndChars()
    {
        var pattern = new OSCAddressPattern("/{test?,data*,[a-z]oo}");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/test1"));
            Assert.That(pattern.IsMatch("/dataLog"));
            Assert.That(pattern.IsMatch("/boo"));
            Assert.That(pattern.IsMatch("/zoo"));
            Assert.That(!pattern.IsMatch("/test12"));
        }
    }

    [Test]
    public void MatchOnlyAsterisk()
    {
        var pattern = new OSCAddressPattern("/*");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/"));
            Assert.That(pattern.IsMatch("/x"));
            Assert.That(pattern.IsMatch("/xyz123"));
        }
    }

    [Test]
    public void MatchQuestionMarkAtEnd()
    {
        var pattern = new OSCAddressPattern("/command?");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/commanda"));
            Assert.That(pattern.IsMatch("/command1"));
            Assert.That(!pattern.IsMatch("/command"));
            Assert.That(!pattern.IsMatch("/command12"));
        }
    }

    [Test]
    public void MatchOnlyCharClass()
    {
        var pattern = new OSCAddressPattern("/[abc]");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/a"));
            Assert.That(pattern.IsMatch("/b"));
            Assert.That(pattern.IsMatch("/c"));
            Assert.That(!pattern.IsMatch("/d"));
        }
    }

    [Test]
    public void MatchEmptyAlternativeOption()
    {
        var pattern = new OSCAddressPattern("/{,start}");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pattern.IsMatch("/"));
            Assert.That(pattern.IsMatch("/start"));
            Assert.That(!pattern.IsMatch("/star"));
        }
    }
}