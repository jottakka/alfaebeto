using Godot;
using Godot.Collections;
using WordProcessing.Models.Rules;

[GlobalClass]
public sealed partial class WordCategoryInfoResource : Resource
{
    [Export]
    public RuleType RuleType { get; set; }
    [Export]
    public Dictionary<string, WordAccuracyInfoResource> WordAccuracyInfos { get; set; } = new();
}
