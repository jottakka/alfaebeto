using Alfaebeto;
using AlfaEBetto.Data.Rules;
using AlfaEBetto.Data.Rules.Rules;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data.Words;

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

	private Array<DiactricalMarkRuleType> _unlockedDiactricalMarkRules =>
		Global.Instance.UserDataInfoResource.UnlockedDiactricalMarksSubCategories;
	private Array<SpellingRuleRuleType> _unlockedSpellingRuleRules =>
		Global.Instance.UserDataInfoResource.UnlockedSpellingRuleRuleTypes;

	public void SetData(BaseRuleItemResource detailedRule)
	{
		bool isUnlocked = false;

		switch (detailedRule)
		{
			case DiactricalMarkRuleItemResource diactricalMarkRuleItemResource:
				isUnlocked = _unlockedDiactricalMarkRules.Contains(diactricalMarkRuleItemResource.RuleType);
				break;
			case SpellingRuleRuleItemResource spellingRuleItemResource:
				isUnlocked = _unlockedSpellingRuleRules.Contains(spellingRuleItemResource.RuleType);
				break;
		}

		LockInfoLabel.Text = isUnlocked
			? "Desbloqueada"
			: "Bloqueada";
		LockedColorRect.Visible = isUnlocked is false;
		LockTextureRect.Texture = isUnlocked
			? GD.Load<Texture2D>(UnlockedIconTexture)
			: GD.Load<Texture2D>(LockedIconTexture);
		ProcessMode = ProcessModeEnum.Always;
		RuleNameLabel.Text = detailedRule.Rule;
		GoToRuleButton.Pressed += () => BuildRuleDescriptionScene(detailedRule);
	}

	private void BuildRuleDescriptionScene(BaseRuleItemResource detailedRule)
	{
		RuleDescriptionUi detailedRuleUi = RuleDescriptionUiPackedScene.Instantiate<RuleDescriptionUi>();
		detailedRuleUi.SetData(detailedRule);
		GetTree().Root.AddChild(detailedRuleUi);
	}
}
