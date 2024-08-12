using Godot;

public sealed partial class MainMenuUi : Node
{
	[Export]
	public Button StartButton { get; set; }
	[Export]
	public Button StoreButton { get; set; }
	[Export]
	public Button RulesButton { get; set; }
	[Export]
	public PackedScene StartGamePackedScene { get; set; }
	[Export]
	public UiComponent UiComponent { get; set; }

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		StartButton.Pressed += () =>
		{
			_ = GetTree().ChangeSceneToPacked(StartGamePackedScene);
		};

		StoreButton.Pressed += () =>
		{
			UiComponent.OpenRuleStoreUi();
		};

		RulesButton.Pressed += () =>
		{
			UiComponent.OpenRuleSetsViewingUi();
		};
	}
}
