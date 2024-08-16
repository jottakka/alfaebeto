using System.Linq;
using Godot;
using WordProcessing.Models.Rules;

public sealed partial class RuleSetsViewingUi : Control
{
	[Export]
	public PackedScene RuleSetListItemPackedScene { get; set; }
	[Export]
	public VBoxContainer RuleListVBoxContainer { get; set; }
	[Export]
	public Button ExitButton { get; set; }
	[Export]
	public CategoryType Category { get; set; }

	private RulesResource _rulesResource => Global.Instance.RulesResource;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		BuildItens();
		ExitButton.Pressed += QueueFree;
	}

	private void BuildItens()
	{
		switch (Category)
		{
			case CategoryType.Acentuation:
				UseDiactricalMarkRules();
				break;
			default:
				UseSpellingRuleRules();
				break;
		}
	}

	private void UseDiactricalMarkRules()
	{
		foreach (BaseRuleSetItemResource ruleSet in _rulesResource.DiactricalMarkRuleSets)
		{
			AddItensToVBox(ruleSet);
		}
	}

	private void UseSpellingRuleRules()
	{
		foreach (BaseRuleSetItemResource ruleSet in _rulesResource.SpellingRuleRuleSets.Where(r => r.CategoryType == Category))
		{
			AddItensToVBox(ruleSet);
		}
	}

	private void AddItensToVBox(BaseRuleSetItemResource ruleSet)
	{
		RuleSetListItem ruleListItem = RuleSetListItemPackedScene.Instantiate<RuleSetListItem>();
		ruleListItem.SetData(ruleSet);
		RuleListVBoxContainer.AddChildDeffered(ruleListItem);
	}
}
