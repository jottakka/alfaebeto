using Godot;
using WordProcessing.Models.Rules;

public sealed partial class RulesViewingUi : Control
{
	[Export]
	public Label RuleSetNameLabel { get; set; }
	[Export]
	public RichTextLabel RuleDescriptionLabel { get; set; }
	[Export]
	public PackedScene RuleListItemPackedScene { get; set; }
	[Export]
	public VBoxContainer RuleListVBoxContainer { get; set; }
	[Export]
	public Button ExitButton { get; set; }

	public override void _Ready()
	{
		ExitButton.Pressed += QueueFree;
	}

	public void SetData(RuleSetListItemViewModel ruleSetListItemViewModel)
	{
		RuleSetNameLabel.Text = ruleSetListItemViewModel.RuleSetName;
		RuleDescriptionLabel.Text = ruleSetListItemViewModel.RichTextDescription;
		foreach (RuleListItemViewModel ruleListItemModel in ruleSetListItemViewModel.Rules)
		{
			AddItemsToVBox(ruleListItemModel);
		}
	}

	private void AddItemsToVBox(RuleListItemViewModel ruleListItemModel)
	{
		RuleListItem ruleListItem = RuleListItemPackedScene.Instantiate<RuleListItem>();
		ruleListItem.SetData(ruleListItemModel);
		RuleListVBoxContainer.AddChildDeffered(ruleListItem);
	}
}
