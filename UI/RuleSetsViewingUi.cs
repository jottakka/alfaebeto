using System.Collections.Generic;
using Godot;
using WordProcessing.Models.Rules;

public sealed partial class RuleSetsViewingUi : Control
{
    [Export]
    public PackedScene RuleListItemPackedScene { get; set; }
    [Export]
    public PackedScene RulesViewingUiItemPackedScene { get; set; }
    [Export]
    public VBoxContainer RuleListVBoxContainer { get; set; }
    private IReadOnlyList<RuleSetListItemViewModel> _ruleSetListItemViewModels => Global.Instance.RuleSetListItems;

    public override void _Ready()
    {
        BuildItens();
    }

    private void BuildItens()
    {
        foreach (RuleSetListItemViewModel ruleSetListItemViewModel in _ruleSetListItemViewModels)
        {
            _ = AddItensToVBox(ruleSetListItemViewModel);
        }
    }

    private void OnPressed(RuleSetListItemViewModel ruleSetListItemViewModel)
    {

        foreach (RuleListItemViewModel ruleListItemModel in ruleSetListItemViewModel.Rules)
        {
            RuleListItem ruleListItem = RuleListItemPackedScene.Instantiate<RuleListItem>();
            ruleListItem.SetData(ruleListItemModel);
            RuleListVBoxContainer.AddChildDeffered(ruleListItem);
        }
    }

    private RuleListItem AddItensToVBox(RuleSetListItemViewModel ruleSetListItemViewModel)
    {
        RuleListItem ruleListItem = RuleListItemPackedScene.Instantiate<RuleListItem>();
        ruleListItem.SetData(ruleSetListItemViewModel);
        ruleListItem.GoToRuleButton.Pressed += () => OnPressed(ruleSetListItemViewModel);
        RuleListVBoxContainer.AddChildDeffered(ruleListItem);
        return ruleListItem;
    }
}
