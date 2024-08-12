using Godot;

[GlobalClass]
public sealed partial class RulesResource : BaseDataResource
{
    [Export]
    public DiactricalMarkRuleSetItemResource[] DiactricalMarkRuleSets { get; set; }
}
