using Godot;
using WordProcessing.Models.DiacriticalMarks;

[GlobalClass]
public partial class DiactricalMarkRuleItemResource : Resource
{
    [Export]
    public DiactricalMarkCategoryType RuleSetType { get; set; }
    [Export]
    public string RuleSet { get; set; }
    [Export]
    public DiactricalMarkSubCategoryType SubRuleType { get; set; }
    [Export]
    public string SubRule { get; set; }
    [Export]
    public bool IsUnlocked { get; set; }
    [Export]
    public int KeyGemCost { get; set; } = 5;
}
