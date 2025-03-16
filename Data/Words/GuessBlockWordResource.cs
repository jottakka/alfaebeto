using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;

public sealed partial class GuessBlockWordResource : Resource
{
	public static string[] Pool = // Full pool of primary Hiragana → Romaji, just the Romaji side
    {
        "a","i","u","e","o",
        "ka","ki","ku","ke","ko",
        "sa","shi","su","se","so",
        "ta","chi","tsu","te","to",
        "na","ni","nu","ne","no",
        "ha","hi","fu","he","ho",
        "ma","mi","mu","me","mo",
        "ya","yu","yo",
        "ra","ri","ru","re","ro",
        "wa","wo",
        "n"
    };

    [Export]
	public CategoryType RuleType { get; set; } = CategoryType.GuessBlock;
	[Export]
	public GuessBlockRuleType DiactricalMarkSubCategoryType { get; set; }
	[Export]
	public string ToBeGuessed { get; set; }
    [Export]
	public string Answer { get; set; }
    
    public (int AnsIdx, string[] Options) GetWordOptions(int incorrectOptionsCount)
    {
        var options = new HashSet<string>();
        while (options.Count < incorrectOptionsCount)
        {
            int randIdx = GD.RandRange(0, Pool.Length - 1);
            options.Add( Pool[randIdx]);
        }

        options.Add(Answer);

        var arrOptions = options.OrderBy(_=>GD.Randi()).ToArray();

        var answerIdx = Array.IndexOf(arrOptions, Answer);

        return (answerIdx, arrOptions);
    }
}