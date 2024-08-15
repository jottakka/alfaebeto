using Godot;
using WordProcessing.Models.Rules;
using WordProcessing.Models.SpellingRules;

public sealed partial class SpellingRuleWordResource : Resource
{
    [Export]
    public RuleType RuleType { get; set; }
    [Export]
    public SpellingRuleRuleType SpellingRuleType { get; set; }
    [Export]
    public string Original { get; set; }
    [Export]
    public string[] Options { get; set; }
    [Export]
    public string RightOption { get; set; }
    [Export]
    public string FirstPart { get; set; }
    [Export]
    public string SecondPart { get; set; }
}
