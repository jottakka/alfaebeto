using System.Collections.Generic;
using Godot;
using WordProcessing.Models.Rules;

public sealed partial class RuleSetsViewingUi : Control
{
	[Export]
	public PackedScene RuleSetListItemPackedScene { get; set; }
	[Export]
	public VBoxContainer RuleListVBoxContainer { get; set; }
	[Export]
	public Button ExitButton { get; set; }
	private IReadOnlyList<RuleSetListItemViewModel> _ruleSetListItemViewModels => Global.Instance.RuleSetListItems;

	public override void _Ready()
	{
		BuildItens();
		ExitButton.Pressed += QueueFree;
	}

	private void BuildItens()
	{
		foreach (RuleSetListItemViewModel ruleSetListItemViewModel in _ruleSetListItemViewModels)
		{
			AddItensToVBox(ruleSetListItemViewModel);
		}
	}

	private void AddItensToVBox(RuleSetListItemViewModel ruleSetListItemViewModel)
	{
		RuleSetListItem ruleListItem = RuleSetListItemPackedScene.Instantiate<RuleSetListItem>();
		ruleListItem.SetData(ruleSetListItemViewModel);
		RuleListVBoxContainer.AddChildDeffered(ruleListItem);
	}
}
