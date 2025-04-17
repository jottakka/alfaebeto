using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Data.Rules;
using AlfaEBetto.Extensions;
using Godot;
using WordProcessing.Models.Rules;

namespace AlfaEBetto.Data.Words;

public sealed partial class RuleCategoriesViewingUi : Control
{
	[Export]
	public PackedScene RuleCategoryListItemPackedScene { get; set; }

	[Export]
	public VBoxContainer RuleCategoryListVBoxContainer { get; set; }
	[Export]
	public Button ExitButton { get; set; }

	private RulesResource _rulesResource => Global.Instance.RulesResource;

	public override void _Ready() => ExitButton.Pressed += QueueFree;

	public void SetData(bool isStore)
	{
		AddItensToVBox(CategoryType.Acentuation, _rulesResource.DiactricalMarkRuleSets, isStore);

		IEnumerable<(CategoryType Category, SpellingRuleRuleSetItemResource[] RuleSets)> ruleSetGroups =
			_rulesResource
			.SpellingRuleRuleSets
			.GroupBy(r => r.CategoryType)
			.Select(g => (Category: g.Key, RuleSets: g.ToArray()));

		foreach ((CategoryType Category, SpellingRuleRuleSetItemResource[] RuleSets) in ruleSetGroups)
		{
			AddItensToVBox(Category, RuleSets, isStore);
		}
	}

	private void AddItensToVBox(CategoryType category, IEnumerable<BaseRuleSetItemResource> ruleSetItemResources, bool isStore)
	{
		RuleCategoryListItem ruleCategoryListItem = RuleCategoryListItemPackedScene.Instantiate<RuleCategoryListItem>();
		ruleCategoryListItem.SetData(category, ruleSetItemResources, isStore);
		RuleCategoryListVBoxContainer.AddChildDeffered(ruleCategoryListItem);
	}
}
