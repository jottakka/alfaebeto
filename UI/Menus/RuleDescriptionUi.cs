using Godot;

public sealed partial class RuleDescriptionUi : Control
{
	[Export]
	public Label RuleNameLabel { get; set; }
	[Export]
	public RichTextLabel ExamplesRichTextLabel { get; set; }
	[Export]
	public RichTextLabel DescriptionRichTextLabel { get; set; }
	[Export]
	public Button BackButton { get; set; }

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		BackButton.Pressed += QueueFree;
	}

	public void SetData(BaseRuleItemResource detailedRule)
	{
		RuleNameLabel.Text = detailedRule.Rule;
		DescriptionRichTextLabel.Text = detailedRule.Description;
		ExamplesRichTextLabel.Text = string.Join(", ", detailedRule.Examples);
	}
}
