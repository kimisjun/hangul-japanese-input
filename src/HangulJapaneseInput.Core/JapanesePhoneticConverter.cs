using System.Text;

namespace HangulJapaneseInput.Core;

public sealed class JapanesePhoneticConverter
{
    private static readonly IReadOnlyDictionary<string, string> BuiltInWords =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["와타시와"] = "わたしは",
            ["와타시"] = "わたし",
            ["콘니치와"] = "こんにちは",
            ["아리가토우고자이마스"] = "ありがとうございます",
            ["아리가토우"] = "ありがとう",
            ["오하요우고자이마스"] = "おはようございます",
            ["오하요우"] = "おはよう",
            ["스미마셍"] = "すみません",
            ["오네가이시마스"] = "おねがいします",
            ["하지메마시테"] = "はじめまして",
            ["도우조요로시쿠"] = "どうぞよろしく",
            ["사요우나라"] = "さようなら",
            ["다이죠우부데스"] = "だいじょうぶです",
            ["하이"] = "はい",
            ["이에"] = "いいえ",
            ["데스"] = "です",
            ["마스"] = "ます"
        };

    private static readonly IReadOnlyDictionary<string, string> KoreanReadingAliases =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["교회"] = "きょうかい",
            ["목사"] = "ぼくし",
            ["예배"] = "れいはい",
            ["성경"] = "せいしょ",
            ["기도"] = "いのり",
            ["하나님"] = "かみさま",
            ["예수님"] = "イエスさま",
            ["찬송가"] = "さんびか"
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> KanjiCandidates =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
        {
            ["にほん"] = ["日本", "二本"],
            ["きょうかい"] = ["教会", "協会"],
            ["せんせい"] = ["先生"],
            ["がくせい"] = ["学生"],
            ["かんこく"] = ["韓国", "勧告"],
            ["せかい"] = ["世界"],
            ["ことば"] = ["言葉"],
            ["かみ"] = ["神", "紙", "髪"],
            ["はな"] = ["花", "鼻"],
            ["あめ"] = ["雨", "飴"],
            ["ぼくし"] = ["牧師"],
            ["れいはい"] = ["礼拝"],
            ["せいしょ"] = ["聖書"],
            ["いのり"] = ["祈り"],
            ["かみさま"] = ["神様"],
            ["イエスさま"] = ["イエス様"],
            ["さんびか"] = ["賛美歌"]
        };

    private static readonly IReadOnlyDictionary<string, string> Mora = CreateMoraMap();
    private readonly IReadOnlyDictionary<string, string> _customWords;

    public JapanesePhoneticConverter(IReadOnlyDictionary<string, string>? customWords = null)
    {
        _customWords = customWords ?? new Dictionary<string, string>();
    }

    public bool TryConvert(string source, out string result)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            result = source;
            return false;
        }

        var bodyLength = source.Length;
        while (bodyLength > 0 && IsTrailingPunctuation(source[bodyLength - 1]))
        {
            bodyLength--;
        }

        var body = source[..bodyLength];
        var suffix = source[bodyLength..];
        if (body.Length == 0)
        {
            result = source;
            return false;
        }

        if (_customWords.TryGetValue(body, out var custom))
        {
            result = custom + suffix;
            return true;
        }

        if (BuiltInWords.TryGetValue(body, out var builtIn))
        {
            result = builtIn + suffix;
            return true;
        }

        if (!TryConvertMora(body, out var converted))
        {
            result = source;
            return false;
        }

        result = converted + suffix;
        return !string.Equals(result, source, StringComparison.Ordinal);
    }

    public bool TryConvertToKatakana(string source, out string result)
    {
        if (!TryConvert(source, out var hiragana))
        {
            result = source;
            return false;
        }

        var output = new StringBuilder(hiragana.Length);
        foreach (var character in hiragana)
        {
            output.Append(character is >= '\u3041' and <= '\u3096'
                ? (char)(character + 0x60)
                : character);
        }

        result = output.ToString();
        return true;
    }

    public bool TryGetKanjiCandidates(string source, out IReadOnlyList<string> candidates)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            candidates = Array.Empty<string>();
            return false;
        }

        var bodyLength = source.Length;
        while (bodyLength > 0 && IsTrailingPunctuation(source[bodyLength - 1]))
        {
            bodyLength--;
        }

        var body = source[..bodyLength];
        var suffix = source[bodyLength..];
        string reading;
        if (!KoreanReadingAliases.TryGetValue(body, out reading!))
        {
            if (!TryConvert(body, out reading))
            {
                candidates = Array.Empty<string>();
                return false;
            }
        }

        if (!KanjiCandidates.TryGetValue(reading, out var matches))
        {
            candidates = Array.Empty<string>();
            return false;
        }

        candidates = suffix.Length == 0
            ? matches
            : matches.Select(candidate => candidate + suffix).ToArray();
        return true;
    }

    private static bool TryConvertMora(string source, out string result)
    {
        var output = new StringBuilder();
        for (var index = 0; index < source.Length; index++)
        {
            var current = source[index].ToString();
            if (Mora.TryGetValue(current, out var kana))
            {
                output.Append(kana);
                continue;
            }

            if (TryConvertHangulWithCoda(source[index], out kana))
            {
                output.Append(kana);
                continue;
            }

            result = source;
            return false;
        }

        result = output.ToString();
        return output.Length > 0;
    }

    private static bool TryConvertHangulWithCoda(char syllable, out string kana)
    {
        const int hangulStart = 0xAC00;
        const int hangulEnd = 0xD7A3;
        if (syllable < hangulStart || syllable > hangulEnd)
        {
            kana = string.Empty;
            return false;
        }

        var offset = syllable - hangulStart;
        var coda = offset % 28;
        if (coda == 0)
        {
            kana = string.Empty;
            return false;
        }

        var openSyllable = (char)(syllable - coda);
        if (!Mora.TryGetValue(openSyllable.ToString(), out var baseKana))
        {
            kana = string.Empty;
            return false;
        }

        // Korean final ㄴ/ㄹ/ㅁ/ㅇ commonly represents Japanese ん;
        // stop consonants commonly represent a small っ in phonetic spelling.
        var ending = coda switch
        {
            4 or 8 or 16 or 21 => "ん",
            1 or 2 or 3 or 5 or 6 or 7 or 9 or 10 or 11 or 12 or 13 or 14 or 15 or 17 or 18 or 19 or 20 or 22 or 23 or 24 or 25 or 26 or 27 => "っ",
            _ => string.Empty
        };

        kana = baseKana + ending;
        return ending.Length > 0;
    }

    private static bool IsTrailingPunctuation(char value) =>
        char.IsPunctuation(value) || value is '…' or '·';

    private static IReadOnlyDictionary<string, string> CreateMoraMap()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["아"]="あ", ["이"]="い", ["우"]="う", ["에"]="え", ["오"]="お",
            ["카"]="か", ["키"]="き", ["쿠"]="く", ["케"]="け", ["코"]="こ",
            ["사"]="さ", ["시"]="し", ["스"]="す", ["세"]="せ", ["소"]="そ",
            ["타"]="た", ["치"]="ち", ["츠"]="つ", ["테"]="て", ["토"]="と",
            ["나"]="な", ["니"]="に", ["누"]="ぬ", ["네"]="ね", ["노"]="の",
            ["하"]="は", ["히"]="ひ", ["후"]="ふ", ["헤"]="へ", ["호"]="ほ",
            ["마"]="ま", ["미"]="み", ["무"]="む", ["메"]="め", ["모"]="も",
            ["야"]="や", ["유"]="ゆ", ["요"]="よ",
            ["라"]="ら", ["리"]="り", ["루"]="る", ["레"]="れ", ["로"]="ろ",
            ["와"]="わ", ["응"]="ん",
            ["가"]="が", ["기"]="ぎ", ["구"]="ぐ", ["게"]="げ", ["고"]="ご",
            ["자"]="ざ", ["지"]="じ", ["즈"]="ず", ["제"]="ぜ", ["조"]="ぞ",
            ["다"]="だ", ["디"]="ぢ", ["두"]="づ", ["데"]="で", ["도"]="ど",
            ["바"]="ば", ["비"]="び", ["부"]="ぶ", ["베"]="べ", ["보"]="ぼ",
            ["파"]="ぱ", ["피"]="ぴ", ["푸"]="ぷ", ["페"]="ぺ", ["포"]="ぽ",
            ["캬"]="きゃ", ["큐"]="きゅ", ["쿄"]="きょ",
            ["샤"]="しゃ", ["슈"]="しゅ", ["쇼"]="しょ",
            ["차"]="ちゃ", ["츄"]="ちゅ", ["쵸"]="ちょ",
            ["냐"]="にゃ", ["뉴"]="にゅ", ["뇨"]="にょ",
            ["햐"]="ひゃ", ["휴"]="ひゅ", ["효"]="ひょ",
            ["먀"]="みゃ", ["뮤"]="みゅ", ["묘"]="みょ",
            ["랴"]="りゃ", ["류"]="りゅ", ["료"]="りょ",
            ["갸"]="ぎゃ", ["규"]="ぎゅ", ["교"]="ぎょ",
            ["쟈"]="じゃ", ["쥬"]="じゅ", ["죠"]="じょ",
            ["뱌"]="びゃ", ["뷰"]="びゅ", ["뵤"]="びょ",
            ["퍄"]="ぴゃ", ["퓨"]="ぴゅ", ["표"]="ぴょ"
        };
        return map;
    }
}
