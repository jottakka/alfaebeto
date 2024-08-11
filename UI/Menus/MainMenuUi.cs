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
	public PackedScene StorePackedScene { get; set; }
	[Export]
	public PackedScene RulesPackedScene { get; set; }

	public override void _Ready()
	{
		StartButton.Pressed += () =>
		{
			_ = GetTree().ChangeSceneToPacked(StartGamePackedScene);
		};

		StoreButton.Pressed += () =>
		{
		};

		RulesButton.Pressed += () =>
		{
		};
	}
}
