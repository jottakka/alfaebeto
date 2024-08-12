using System.Linq;
using Godot;
using WordProcessing.Models.DiacriticalMarks;

[GlobalClass]
public partial class DiactricalMarkRuleSetItemResource : Resource
{
    [Export]
    public DiactricalMarkCategoryType RuleSetType { get; set; }
    [Export]
    public string Name { get; set; }
    [Export]
    public DiactricalMarkRuleItemResource[] Rules { get; set; }
    [Export]
    public string Description { get; set; }

    public int UnlockedRulesCount => Rules.Count(rule => rule.IsUnlocked);

    public int TotalRulesCount => Rules.Count();
}
