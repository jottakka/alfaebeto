using Godot;

public sealed partial class SceneManagerComponent : Node
{
	[Export]
	public PackedScene StartGamePackedScene { get; set; }
	[Export]
	public PackedScene MainMenuPackedScene { get; set; }

	public void SwitchToMainMenu()
	{
		_ = GetTree().ChangeSceneToPacked(MainMenuPackedScene);
	}

	public void SwitchToStartGame()
	{
		_ = GetTree().ChangeSceneToPacked(StartGamePackedScene);
	}
}
