namespace HangulJapaneseInput.Core;

public enum ConversionCommand
{
    None,
    Hiragana,
    Katakana,
    Kanji
}

public static class ConversionShortcut
{
    private const int VkSpace = 0x20;
    private const int VkF7 = 0x76;

    public static ConversionCommand Resolve(
        int virtualKey,
        bool control,
        bool shift,
        bool alt)
    {
        if (shift || alt)
        {
            return ConversionCommand.None;
        }

        if (virtualKey == VkSpace)
        {
            return control ? ConversionCommand.Kanji : ConversionCommand.Hiragana;
        }

        if (virtualKey == VkF7 && !control)
        {
            return ConversionCommand.Katakana;
        }

        return ConversionCommand.None;
    }
}
