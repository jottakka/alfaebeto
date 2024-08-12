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
		BackButton.Pressed += QueueFree;
	}

	public void SetData(DiactricalMarkRuleItemResource detailedRule)
	{
		RuleNameLabel.Text = detailedRule.Name;
		DescriptionRichTextLabel.Text = detailedRule.Description;
		ExamplesRichTextLabel.Text = string.Join(", ", detailedRule.Examples);
	}
}
