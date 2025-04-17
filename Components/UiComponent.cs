using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Components
{
	public sealed partial class UiComponent : Node
	{
		[Export]
		public PackedScene RuleCategoryStorePackaeScene { get; set; }
		[Export]
		public PackedScene RuleCategoriesViewingUiPackedScene { get; set; }

		public void OpenRuleStoreUi()
		{
			RuleCategoriesViewingUi ruleStore = RuleCategoriesViewingUiPackedScene.Instantiate<RuleCategoriesViewingUi>();
			ruleStore.SetData(isStore: true);
			GetTree().Root.AddChild(ruleStore);
		}

		public void OpenRuleSetsViewingUi()
		{
			RuleCategoriesViewingUi rulesViewing = RuleCategoriesViewingUiPackedScene.Instantiate<RuleCategoriesViewingUi>();
			rulesViewing.SetData(isStore: false);
			GetTree().Root.AddChild(rulesViewing);
		}

		private void OpenUi<TUi>(PackedScene packedScene)
			where TUi : Control
		{
			TUi ruleStore = packedScene.Instantiate<TUi>();
			GetTree().Root.AddChild(ruleStore);
		}
	}
}
