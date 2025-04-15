using Godot;
using WordProcessing.Models.Rules;

[GlobalClass]
public sealed partial class WordCategoryInfoResource : Resource
{
	[Export]
	public CategoryType RuleType { get; set; }
	[Export]
	public Godot.Collections.Dictionary<string, WordAccuracyInfoResource> WordAccuracyInfos { get; set; } = [];
}
