using Godot;
using WordProcessing.Models.Rules;
using WordProcessing.Models.SpellingRules;

[GlobalClass]
public partial class SpellingRuleItemResource : Resource
{
    [Export]
    public RuleType RuleSetType { get; set; }
    [Export]
    public string RuleSet { get; set; }
    [Export]
    public SpellingRuleRuleType RuleType { get; set; }
    [Export]
    public string Name { get; set; }
    [Export]
    public string Description { get; set; }
    [Export]
    public string[] Examples { get; set; }
    [Export]
    public bool IsUnlocked { get; set; }
    [Export]
    public int KeyGemCost { get; set; } = 5;

    [Signal]
    public delegate void OnUnlockSignalEventHandler();

    public void Unlock()
    {
        IsUnlocked = true;
        _ = EmitSignal(nameof(OnUnlockSignal));
    }
}
