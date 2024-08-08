using Godot;
using WordProcessing.Models.Rules;

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

	public void SetData(DetailedRuleViewModel data)
	{
		RuleNameLabel.Text = data.RuleName;
		DescriptionRichTextLabel.Text = data.RuleDescriptionRichText;
		ExamplesRichTextLabel.Text = string.Join(", ", data.Examples);
	}
}
