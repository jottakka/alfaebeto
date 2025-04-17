using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Data.Rules;
using AlfaEBetto.Data.Rules.Rules;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data.Words;

public sealed partial class RuleCategoryListItem : MarginContainer
{
	[Export]
	public Label CategoryNameLabel { get; set; }
	[Export]
	public Label TotalCategoriesCountLabel { get; set; }
	[Export]
	public Label UnlockedRulesCountLabel { get; set; }
	[Export]
	public Button GoToCategoryButton { get; set; }
	[Export]
	public PackedScene RuleSetsViewingUiPackedScene { get; set; }
	[Export]
	public PackedScene RuleStoreUiPackedScene { get; set; }
	[Export]
	public ColorRect LockedColorRect { get; set; }
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

	public override void _Ready() => ProcessMode = ProcessModeEnum.Always;

	public void SetData(CategoryType category, IEnumerable<BaseRuleSetItemResource> ruleSets, bool isStoreScene)
	{
		int unlockedCount = GetUnlockedCount(category, ruleSets);

		ProcessMode = ProcessModeEnum.Always;
		CategoryNameLabel.Text = category.GetCategoryName();
		LockedColorRect.Visible = isStoreScene is false && unlockedCount == 0;
		LockTextureRect.Texture = unlockedCount > 0
			? GD.Load<Texture2D>(UnlockedIconTexture)
			: GD.Load<Texture2D>(LockedIconTexture);
		TotalCategoriesCountLabel.Text =
			ruleSets.Sum(set => set.TotalRulesCount).ToString();
		UnlockedRulesCountLabel.Text = unlockedCount.ToString();
		GoToCategoryButton.Pressed += () => BuildScene(category, ruleSets, isStoreScene);
	}

	private void BuildScene(CategoryType category, IEnumerable<BaseRuleSetItemResource> ruleSets, bool isStoreScene)
	{
		if (isStoreScene is true)
		{
			RuleStoreUi ruleStoreUi = RuleStoreUiPackedScene.Instantiate<RuleStoreUi>();
			ruleStoreUi.SetCategory(category);
			GetTree().Root.AddChild(ruleStoreUi);
		}
		else
		{
			BuildRuleSetViewingUiScene(category, ruleSets);
		}
	}

	private void BuildRuleSetViewingUiScene(CategoryType category, IEnumerable<BaseRuleSetItemResource> ruleSets)
	{
		RuleSetsViewingUi ruleSetsViewing = RuleSetsViewingUiPackedScene.Instantiate<RuleSetsViewingUi>();
		ruleSetsViewing.SetData(category, ruleSets);
		GetTree().Root.AddChild(ruleSetsViewing);
	}

	private int GetUnlockedCount(CategoryType category, IEnumerable<BaseRuleSetItemResource> ruleSets)
	{
		return category switch
		{
			CategoryType.Acentuation =>
				ruleSets.Sum(
					set => set
						.RulesAsBaseItemResource
						.OfType<DiactricalMarkRuleItemResource>()
						.Count(r => _unlockedDiactricalMarkRules.Contains(r.RuleType))),
			_ =>
				ruleSets.Sum(
					set => set
					.RulesAsBaseItemResource
					.Where(rule => rule.CategoryType == category)
					.OfType<SpellingRuleRuleItemResource>()
					.Count(r => _unlockedSpellingRuleRules.Contains(r.RuleType))),
		};
	}
}
