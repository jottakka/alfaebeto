namespace WordProcessing.Models.DiacriticalMarks;

public enum GuessBlockRuleType
{
    GuessRomangiFromHiragana,
    GuessRomangiFromKatakana,
    GuessHiraganaFromRomangi,
    GuessHiraganaFromKatakana,
    GuessKatakanaFromRomangi,
    GuessKatakanaFromHiragana,
    GuessKanjiFromHiragana,
    GuessHiraganaFromKanji,
}
