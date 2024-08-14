using System.Linq;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

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

	private DiactricalMarkRuleSetItemResource _ruleSet;
	private Array<DiactricalMarkSubCategoryType> _unlockedRules => Global.Instance.UserDataInfoResource.UnlockedDiactricalMarksSubCategories;

	public void SetData(DiactricalMarkRuleSetItemResource ruleSet)
	{
		_ruleSet = ruleSet;

		int unlockedCount = GetUnlockedCount();

		ProcessMode = ProcessModeEnum.Always;
		RuleNameLabel.Text = ruleSet.Name;
		LockedColorRect.Visible = unlockedCount == 0;
		LockTextureRect.Texture = unlockedCount > 0
			? GD.Load<Texture2D>(UnlockedIconTexture)
			: GD.Load<Texture2D>(LockedIconTexture);
		TotalRulesCountLabel.Text = ruleSet.TotalRulesCount.ToString();
		UnlockedRulesCountLabel.Text = unlockedCount.ToString();
		GoToRuleButton.Pressed += () => BuildRuleListItemScene(ruleSet);
	}

	private void BuildRuleListItemScene(DiactricalMarkRuleSetItemResource ruleSet)
	{
		RulesViewingUi rulesViewing = RuleViewingUiPackedScene.Instantiate<RulesViewingUi>();
		rulesViewing.SetData(ruleSet);
		GetTree().Root.AddChild(rulesViewing);
	}

	private int GetUnlockedCount()
	{
		return _ruleSet.Rules.Count(r => _unlockedRules.Contains(r.RuleType));
	}
}
