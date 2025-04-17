using AlfaEBetto.Data.Rules;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Data.Words
{
	public sealed partial class RulesViewingUi : Control
	{
		[Export]
		public Label RuleSetNameLabel { get; set; }
		[Export]
		public RichTextLabel RuleDescriptionLabel { get; set; }
		[Export]
		public PackedScene RuleListItemPackedScene { get; set; }
		[Export]
		public VBoxContainer RuleListVBoxContainer { get; set; }
		[Export]
		public Button ExitButton { get; set; }

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
			ExitButton.Pressed += QueueFree;
		}

		public void SetData(BaseRuleSetItemResource ruleSet)
		{
			RuleSetNameLabel.Text = ruleSet.RuleSet;
			RuleDescriptionLabel.Text = ruleSet.Description;
			foreach (BaseRuleItemResource ruleListItemModel in ruleSet.RulesAsBaseItemResource)
			{
				AddItemsToVBox(ruleListItemModel);
			}
		}

		private void AddItemsToVBox(BaseRuleItemResource rule)
		{
			RuleListItem ruleListItem = RuleListItemPackedScene.Instantiate<RuleListItem>();
			ruleListItem.SetData(rule);
			RuleListVBoxContainer.AddChildDeffered(ruleListItem);
		}
	}
}
