using Godot;
using WordProcessing.Enums;
using WordProcessing.Models.Rules;

namespace AlfaEBetto.Data.Words;

public sealed partial class GuessBlockWordResource : Resource
{
	[Export]
	public CategoryType RuleType { get; set; } = CategoryType.GuessBlock;
	[Export]
	public GuessBlockRuleType DiactricalMarkSubCategoryType { get; set; }
	[Export]
	public string ToBeGuessed { get; set; }

	public int AnswerIdx { get; set; }

	public string[] ShuffledOptions { get; set; }
}
