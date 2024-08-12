using Godot;

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
		ProcessMode = ProcessModeEnum.Always;
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		ExitButton.Pressed += QueueFree;
	}

	public void SetData(DiactricalMarkRuleSetItemResource ruleSet)
	{
		RuleSetNameLabel.Text = ruleSet.Name;
		RuleDescriptionLabel.Text = ruleSet.Description;
		foreach (DiactricalMarkRuleItemResource ruleListItemModel in ruleSet.Rules)
		{
			AddItemsToVBox(ruleListItemModel);
		}
	}

	private void AddItemsToVBox(DiactricalMarkRuleItemResource rule)
	{
		RuleListItem ruleListItem = RuleListItemPackedScene.Instantiate<RuleListItem>();
		ruleListItem.SetData(rule);
		RuleListVBoxContainer.AddChildDeffered(ruleListItem);
	}
}
