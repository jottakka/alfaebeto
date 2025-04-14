using Godot;
using WordProcessing.Enums;

public partial class Global : Node
{
	public static Global Instance { get; private set; } = null;

	public UserDataManagementComponent UserDataManagementComponent { get; set; }

	public UserDataInfoResource UserDataInfoResource => _dataResourceManager.UserDataInfoResource;

	public RulesResource RulesResource => _dataResourceManager.RulesResource;

	public DiactricalMarkWordsDataResource DiactricalMarkWordsDataResource => _dataResourceManager.DiactricalMarkWordsDataResource;

    public GuessBlockWordsDataResource GuessBlocksDataResource => _dataResourceManager.GuessBlocksWordsDataResource;

    public SpellingRulesResource SpellingRulesResource => _dataResourceManager.SpellingRulesResource;

	public SupportedLanguage SupportedLanguage => SupportedLanguage.German;

	public Player Player { get; private set; }

	public StageBase Scene { get; private set; }

	public Node CurrentScene { get; private set; }

	public SpellingRuleWordResource GetNextSpellingRuleWordResource() => _wordServerManager.GetNextSpellingRuleWord();
	public DiactricalMarkWordResource GetGetNextDiactricalMarkRuleWordResource() => _wordServerManager.GetNextDiactricalMarkWord();

    public GuessBlockWordResource GetGetNextGuessBlockWordResource() => _wordServerManager.GetNextGuessBlockWordResource();


    [Signal]
	public delegate void OnMainNodeSetupFinishedSignalEventHandler();

	private WordServerManager _wordServerManager { get; } = new();
	private DataResourceManager _dataResourceManager { get; } = new();

	public override void _Ready()
	{
		if (Instance is not null)
		{
			if (Instance != this)
			{
				GD.Print("Global instance already exists, destroying duplicate instance.");
			}

			QueueFree();

			return;
		}

		Instance = this;
		// Initialize random number generator
		GD.Randomize();
		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);
	}

	public void SettingMainNodeData(Player player, StageBase stage)
	{
		Player = player;
		Scene = stage;
		_ = EmitSignal(nameof(OnMainNodeSetupFinishedSignal));
	}

	public void SwitchToMainMenu()
	{
		GotoScene("res://UI/Menus/main_menu.tscn");
	}

	public void SwitchToStartGame()
	{
		GotoScene("res://start_game.tscn");
	}

	private void GotoScene(string path)
	{
		// This function will usually be called from a signal callback,
		// or some other function from the current scene.
		// Deleting the current scene at this point is
		// a bad idea, because it may still be executing code.
		// This will result in a crash or unexpected behavior.

		// The solution is to defer the load to a later time, when
		// we can be sure that no code from the current scene is running:

		_ = CallDeferred(MethodName.DeferredGotoScene, path);
	}

	private void DeferredGotoScene(string path)
	{
		// It is now safe to remove the current scene.
		CurrentScene.Free();

		// Load a new scene.
		PackedScene nextScene = GD.Load<PackedScene>(path);

		// Instance the new scene.
		CurrentScene = nextScene.Instantiate();

		// Add it to the active scene, as child of root.
		GetTree().Root.AddChild(CurrentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
		GetTree().CurrentScene = CurrentScene;
	}
}
