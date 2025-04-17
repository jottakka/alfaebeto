using WordProcessing.Enums;
using WordProcessing.Util;

namespace AlfaEBetto.WordProcessing.Util
{
	public static class JapaneseKanaUtil
	{
		private static Random _random = new();

		private static readonly GuessBlockRuleType[] ValidRuleTypes =
		{
			GuessBlockRuleType.GuessKatakanaFromRomangiOptions,
			GuessBlockRuleType.GuessHiraganaFromRomangiOptions,
			GuessBlockRuleType.GuessKatakanaFromHiraganaOptions,
			GuessBlockRuleType.GuessRomangiFromHiraganaOptions,
			GuessBlockRuleType.GuessHiraganaFromKatakanaOptions,
			GuessBlockRuleType.GuessRomangiFromKatakanaOptions
		};

		private static string[] RomajiPool =
		{
            // --- Basic 46 (gojūon) ---
            "a","i","u","e","o",
			"ka","ki","ku","ke","ko",
			"sa","shi","su","se","so",
			"ta","chi","tsu","te","to",
			"na","ni","nu","ne","no",
			"ha","hi","fu","he","ho",
			"ma","mi","mu","me","mo",
			"ya","yu","yo",
			"ra","ri","ru","re","ro",
			"wa","wo","n",
    
            // --- Dakuon (5 groups × 5 each = 25) ---
            "ga","gi","gu","ge","go",
			"za","ji","zu","ze","zo",
			"da","ji","zu","de","do",
			"ba","bi","bu","be","bo",
    
            // --- Handakuon (p line, 5) ---
            "pa","pi","pu","pe","po"
		};

		private static string[] HiraganaPool =
		{
            // --- Basic 46 ---
            "あ","い","う","え","お",
			"か","き","く","け","こ",
			"さ","し","す","せ","そ",
			"た","ち","つ","て","と",
			"な","に","ぬ","ね","の",
			"は","ひ","ふ","へ","ほ",
			"ま","み","む","め","も",
			"や","ゆ","よ",
			"ら","り","る","れ","ろ",
			"わ","を","ん",
    
            // --- Dakuon ---
            "が","ぎ","ぐ","げ","ご",
			"ざ","じ","ず","ぜ","ぞ",
			"だ","ぢ","づ","で","ど",
			"ば","び","ぶ","べ","ぼ",
    
            // --- Handakuon ---
            "ぱ","ぴ","ぷ","ぺ","ぽ"
		};

		private static string[] KatakanaPool =
		{
            // --- Basic 46 ---
            "ア","イ","ウ","エ","オ",
			"カ","キ","ク","ケ","コ",
			"サ","シ","ス","セ","ソ",
			"タ","チ","ツ","テ","ト",
			"ナ","ニ","ヌ","ネ","ノ",
			"ハ","ヒ","フ","ヘ","ホ",
			"マ","ミ","ム","メ","モ",
			"ヤ","ユ","ヨ",
			"ラ","リ","ル","レ","ロ",
			"ワ","ヲ","ン",
    
            // --- Dakuon ---
            "ガ","ギ","グ","ゲ","ゴ",
			"ザ","ジ","ズ","ゼ","ゾ",
			"ダ","ヂ","ヅ","デ","ド",
			"バ","ビ","ブ","ベ","ボ",
    
            // --- Handakuon ---
            "パ","ピ","プ","ペ","ポ"
		};

		public static PickRightOptionFromHintData GetRandomRuleData(int wrongOptionsCount)
		{
			int ruleIdx = _random.Next(ValidRuleTypes.Count());

			return GetData(ValidRuleTypes[ruleIdx], wrongOptionsCount);
		}

		public static PickRightOptionFromHintData GetData(GuessBlockRuleType ruleType, int wrongOptionsCount)
		{
			(string[]? hintPool, string[]? optionsPool) = ruleType switch
			{
				GuessBlockRuleType.GuessKatakanaFromRomangiOptions => (KatakanaPool, RomajiPool),
				GuessBlockRuleType.GuessHiraganaFromRomangiOptions => (HiraganaPool, RomajiPool),

				GuessBlockRuleType.GuessKatakanaFromHiraganaOptions => (KatakanaPool, HiraganaPool),
				GuessBlockRuleType.GuessRomangiFromHiraganaOptions => (RomajiPool, HiraganaPool),

				GuessBlockRuleType.GuessHiraganaFromKatakanaOptions => (HiraganaPool, KatakanaPool),
				GuessBlockRuleType.GuessRomangiFromKatakanaOptions => (RomajiPool, KatakanaPool),

				_ => throw new ArgumentOutOfRangeException(nameof(ruleType), ruleType, null)
			};

			return BuildQuestionItem(hintPool, optionsPool, wrongOptionsCount);
		}

		private static PickRightOptionFromHintData BuildQuestionItem(
			string[] hintPool,
			string[] optionsPool,
			int totalOptions
			)
		{
			HashSet<(string Option, string Hint)> shuffledItems = [];

			while (shuffledItems.Count <= totalOptions)
			{
				int randIdx = _random.Next(optionsPool.Length);
				shuffledItems.Add((optionsPool[randIdx], hintPool[randIdx]));
			}

			int answerIdx = _random.Next(shuffledItems.Count);
			string answer = shuffledItems.ElementAt(answerIdx).Hint;
			string[] optionsArr = shuffledItems.Select(x => x.Option).ToArray();

			return new PickRightOptionFromHintData(
				answer,
				answerIdx,
				optionsArr);
		}
	}
}
