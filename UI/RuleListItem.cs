using Godot;
using WordProcessing.Models.Rules;

public sealed partial class RuleListItem : MarginContainer
{
    [Export]
    public Label RuleNameLabel { get; set; }
    [Export]
    public Label TotalRulesCountLabel { get; set; }
    [Export]
    public Label UnlockedRulesCountLabel { get; set; }
    [Export]
    public Button GoToRuleButton { get; set; }

    public void SetData(RuleSetListItemViewModel ruleSetListItemViewModel)
    {
        RuleNameLabel.Text = ruleSetListItemViewModel.RuleSetName;
        TotalRulesCountLabel.Text = ruleSetListItemViewModel.TotalCount.ToString();
        UnlockedRulesCountLabel.Text = ruleSetListItemViewModel.UnlockedCount.ToString();
    }

    public void SetData(RuleListItemViewModel ruleSetListItemViewModel)
    {
        RuleNameLabel.Text = ruleSetListItemViewModel.Name;
        TotalRulesCountLabel.QueueFree();
        UnlockedRulesCountLabel.QueueFree();
        GoToRuleButton.Pressed += () => BuildRuleDescriptionScene(ruleSetListItemViewModel.DetailedModel);
    }

    private void BuildRuleDescriptionScene(DetailedRuleViewModel detailedRuleViewModel)
    {
        RuleDescriptionUi detailedRule = RuleDescriptionUiPackedScene.Instantiate<RuleDescriptionUi>();
        detailedRule.SetData(detailedRuleViewModel);
        GetTree().Root.AddChild(detailedRule);
    }
}
