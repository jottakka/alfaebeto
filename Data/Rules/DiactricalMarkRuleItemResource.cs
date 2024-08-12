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
    public DiactricalMarkSubCategoryType RuleType { get; set; }
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
}
