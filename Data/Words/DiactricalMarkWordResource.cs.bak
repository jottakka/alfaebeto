using Godot;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;

public sealed partial class DiactricalMarkWordResource : Resource
{
	[Export]
	public CategoryType RuleType { get; set; } = CategoryType.Acentuation;
	[Export]
	public DiactricalMarkRuleType DiactricalMarkSubCategoryType { get; set; }
	[Export]
	public string Original { get; set; }
	[Export]
	public string WithoutMark { get; set; }
	[Export]
	public bool HasMark { get; set; }
	[Export]
	public int MarkIndex { get; set; }
}