using System.Linq;
using AlfaEBetto.Data.Rules;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data.Words;

public sealed partial class RuleSetListItem : MarginContainer
{
	[Export]
	public Label RuleNameLabel { get; set; }
	[Export]
	public Label TotalRulesCountLabel { get; set; }
	[Export]
	public Label UnlockedRulesCountLabel { get; set; }
	[Export]
	public Button GoToRuleButton { get; set; }
	[Export]
	public PackedScene RuleViewingUiPackedScene { get; set; }
	[Export]
	public ColorRect LockedColorRect { get; set; }
	[Export]
	public TextureRect LockTextureRect { get; set; }
	[Export(PropertyHint.File)]
	public string UnlockedIconTexture { get; set; }
	[Export(PropertyHint.File)]
	public string LockedIconTexture { get; set; }

	private BaseRuleSetItemResource _ruleSet;
	private Array<DiactricalMarkRuleType> _unlockedDiactricalMarkRules =>
		Global.Instance.UserDataInfoResource.UnlockedDiactricalMarksSubCategories;
	private Array<SpellingRuleRuleType> _unlockedSpellingRuleRules =>
		Global.Instance.UserDataInfoResource.UnlockedSpellingRuleRuleTypes;

	public void SetData(BaseRuleSetItemResource ruleSet)
	{
		_ruleSet = ruleSet;

		int unlockedCount = GetUnlockedCount();

		ProcessMode = ProcessModeEnum.Always;
		RuleNameLabel.Text = ruleSet.RuleSet;
		LockedColorRect.Visible = unlockedCount == 0;
		LockTextureRect.Texture = unlockedCount > 0
			? GD.Load<Texture2D>(UnlockedIconTexture)
			: GD.Load<Texture2D>(LockedIconTexture);
		TotalRulesCountLabel.Text = ruleSet.TotalRulesCount.ToString();
		UnlockedRulesCountLabel.Text = unlockedCount.ToString();
		GoToRuleButton.Pressed += () => BuildRuleListItemScene(ruleSet);
	}

	private void BuildRuleListItemScene(BaseRuleSetItemResource ruleSet)
	{
		RulesViewingUi rulesViewing = RuleViewingUiPackedScene.Instantiate<RulesViewingUi>();
		rulesViewing.SetData(ruleSet);
		GetTree().Root.AddChild(rulesViewing);
	}

	private int GetUnlockedCount()
	{
		return _ruleSet switch
		{
			DiactricalMarkRuleSetItemResource diactricalMarkRuleSetItemResource =>
				diactricalMarkRuleSetItemResource.Rules.Count(r => _unlockedDiactricalMarkRules.Contains(r.RuleType)),
			SpellingRuleRuleSetItemResource spellingRuleRuleSetItemResource =>
				spellingRuleRuleSetItemResource.Rules.Count(r => _unlockedSpellingRuleRules.Contains(r.RuleType)),
			_ => 0,
		};
	}
}
