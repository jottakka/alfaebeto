using Godot;

public sealed partial class UiComponent : Node
{
	[Export]
	public PackedScene RuleStorePackaeScene { get; set; }
	[Export]
	public PackedScene RuleSetsViewingUiPackedScene { get; set; }

	public void OpenRuleStoreUi()
	{
		OpenUi<RuleStoreUi>(RuleStorePackaeScene);
	}

	public void OpenRuleSetsViewingUi()
	{
		OpenUi<RuleSetsViewingUi>(RuleSetsViewingUiPackedScene);
	}

	private void OpenUi<TUi>(PackedScene packedScene)
		where TUi : Control
	{
		TUi ruleStore = packedScene.Instantiate<TUi>();
		GetTree().Root.AddChild(ruleStore);
	}
}
