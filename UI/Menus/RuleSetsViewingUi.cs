using System.Collections.Generic;
using AlfaEBetto.Data.Rules;
using AlfaEBetto.Extensions;
using Godot;
using WordProcessing.Models.Rules;

namespace AlfaEBetto.Data.Words
{
	public sealed partial class RuleSetsViewingUi : Control
	{
		[Export]
		public PackedScene RuleSetListItemPackedScene { get; set; }
		[Export]
		public VBoxContainer RuleListVBoxContainer { get; set; }
		[Export]
		public Button ExitButton { get; set; }
		public CategoryType Category { get; set; }

		private RulesResource _rulesResource => Global.Instance.RulesResource;

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
			ExitButton.Pressed += QueueFree;
		}

		public void SetData(CategoryType categoryType, IEnumerable<BaseRuleSetItemResource> ruleSetResources)
		{
			Category = categoryType;
			foreach (BaseRuleSetItemResource ruleSet in ruleSetResources)
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
}
