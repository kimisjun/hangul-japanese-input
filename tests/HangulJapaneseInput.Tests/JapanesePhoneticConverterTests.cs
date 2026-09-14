using HangulJapaneseInput.Core;

namespace HangulJapaneseInput.Tests;

public class JapanesePhoneticConverterTests
{
    private readonly JapanesePhoneticConverter _converter = new();

    [Theory]
    [InlineData("와타시", "わたし")]
    [InlineData("와타시와", "わたしは")]
    [InlineData("콘니치와", "こんにちは")]
    [InlineData("아리가토우고자이마스", "ありがとうございます")]
    [InlineData("스미마셍", "すみません")]
    public void Converts_common_Korean_pronunciation_to_Japanese(string source, string expected)
    {
        var converted = _converter.TryConvert(source, out var result);

        Assert.True(converted);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Preserves_trailing_punctuation()
    {
        var converted = _converter.TryConvert("와타시와,", out var result);

        Assert.True(converted);
        Assert.Equal("わたしは,", result);
    }

    [Fact]
    public void Does_not_change_unknown_Korean_words()
    {
        var converted = _converter.TryConvert("한글문서", out var result);

        Assert.False(converted);
        Assert.Equal("한글문서", result);
    }

    [Fact]
    public void Custom_dictionary_takes_priority()
    {
        var converter = new JapanesePhoneticConverter(new Dictionary<string, string>
        {
            ["교회"] = "きょうかい"
        });

        var converted = converter.TryConvert("교회", out var result);

        Assert.True(converted);
        Assert.Equal("きょうかい", result);
    }

    [Theory]
    [InlineData("테레비", "テレビ")]
    [InlineData("콘니치와", "コンニチハ")]
    [InlineData("쿄우카이", "キョウカイ")]
    public void Converts_Korean_pronunciation_to_katakana(string source, string expected)
    {
        var converted = _converter.TryConvertToKatakana(source, out var result);

        Assert.True(converted);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("니혼", "日本", "二本")]
    [InlineData("쿄우카이", "教会", "協会")]
    [InlineData("카미", "神", "紙", "髪")]
    public void Returns_kanji_candidates_in_preferred_order(string source, params string[] expected)
    {
        var found = _converter.TryGetKanjiCandidates(source, out var candidates);

        Assert.True(found);
        Assert.Equal(expected, candidates);
    }

    [Theory]
    [InlineData("교회", "教会")]
    [InlineData("목사", "牧師")]
    [InlineData("예배", "礼拝")]
    [InlineData("성경", "聖書")]
    [InlineData("찬송가", "賛美歌")]
    public void Returns_kanji_for_registered_Korean_church_words(string source, string expected)
    {
        var found = _converter.TryGetKanjiCandidates(source, out var candidates);

        Assert.True(found);
        Assert.Equal(expected, candidates[0]);
    }

    [Fact]
    public void Returns_no_kanji_candidates_for_an_unknown_word()
    {
        var found = _converter.TryGetKanjiCandidates("등록안됨", out var candidates);

        Assert.False(found);
        Assert.Empty(candidates);
    }
}
