using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

public sealed partial class RuleListItem : MarginContainer
{
	[Export]
	public Label RuleNameLabel { get; set; }
	[Export]
	public Button GoToRuleButton { get; set; }
	[Export]
	public PackedScene RuleDescriptionUiPackedScene { get; set; }
	[Export]
	public ColorRect LockedColorRect { get; set; }
	[Export]
	public Label LockInfoLabel { get; set; }
	[Export]
	public TextureRect LockTextureRect { get; set; }
	[Export(PropertyHint.File)]
	public string UnlockedIconTexture { get; set; }
	[Export(PropertyHint.File)]
	public string LockedIconTexture { get; set; }

	private Array<DiactricalMarkSubCategoryType> _unlockedRules => Global.Instance.UserDataInfoResource.UnlockedDiactricalMarksSubCategories;

	public void SetData(DiactricalMarkRuleItemResource detailedRule)
	{

		bool isUnlocked = _unlockedRules.Contains(detailedRule.RuleType);

		LockInfoLabel.Text = isUnlocked
			? "Desbloqueada"
			: "Bloqueada";
		LockedColorRect.Visible = isUnlocked is false;
		LockTextureRect.Texture = isUnlocked
			? GD.Load<Texture2D>(UnlockedIconTexture)
			: GD.Load<Texture2D>(LockedIconTexture);
		ProcessMode = ProcessModeEnum.Always;
		RuleNameLabel.Text = detailedRule.Name;
		GoToRuleButton.Pressed += () => BuildRuleDescriptionScene(detailedRule);
	}

	private void BuildRuleDescriptionScene(DiactricalMarkRuleItemResource detailedRule)
	{
		RuleDescriptionUi detailedRuleUi = RuleDescriptionUiPackedScene.Instantiate<RuleDescriptionUi>();
		detailedRuleUi.SetData(detailedRule);
		GetTree().Root.AddChild(detailedRuleUi);
	}
}
