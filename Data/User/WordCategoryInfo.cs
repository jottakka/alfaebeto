using Godot;
using Godot.Collections;
using WordProcessing.Models.Rules;

public sealed partial class WordCategoryInfo : Resource
{
    [Export]
    public RuleType RuleType { get; set; }
    [Export]
    public Dictionary<RuleType, WordAccuracyInfo> WordsCategoryInfos { get; set; } = new();
}
