using HangulJapaneseInput.Core;

namespace HangulJapaneseInput.Tests;

public class ConversionShortcutTests
{
    [Theory]
    [InlineData(0x20, false, false, false, ConversionCommand.Hiragana)]
    [InlineData(0x76, false, false, false, ConversionCommand.Katakana)]
    [InlineData(0x20, true, false, false, ConversionCommand.Kanji)]
    [InlineData(0x20, false, true, false, ConversionCommand.None)]
    [InlineData(0x76, true, false, false, ConversionCommand.None)]
    [InlineData(0x41, false, false, false, ConversionCommand.None)]
    public void Resolves_only_supported_shortcuts(
        int virtualKey,
        bool control,
        bool shift,
        bool alt,
        ConversionCommand expected)
    {
        var actual = ConversionShortcut.Resolve(virtualKey, control, shift, alt);

        Assert.Equal(expected, actual);
    }
}
