using Godot;
using WordProcessing.Enums;

public partial class Global : Node
{
	// Singleton Instance - standard pattern
	public static Global Instance { get; private set; } = null;

	// --- Components & Managers ---
	public UserDataManagementComponent UserDataManagementComponent { get; set; }
	private readonly WordServerManager _wordServerManager = new();
	private readonly DataResourceManager _dataResourceManager = new();

	// --- Data Resource Accessors ---
	public UserDataInfoResource UserDataInfoResource => _dataResourceManager?.UserDataInfoResource;
	public RulesResource RulesResource => _dataResourceManager?.RulesResource;
	public DiactricalMarkWordsDataResource DiactricalMarkWordsDataResource => _dataResourceManager?.DiactricalMarkWordsDataResource;
	public GuessBlockWordsDataResource GuessBlocksDataResource => _dataResourceManager?.GuessBlocksWordsDataResource;
	public SpellingRulesResource SpellingRulesResource => _dataResourceManager?.SpellingRulesResource;

	// --- Game State ---
	public SupportedLanguage CurrentLanguage { get; set; } = SupportedLanguage.German; // Default, load/set as needed
	public Player Player { get; private set; }
	public StageBase Scene { get; private set; }
	public Node CurrentSceneNode { get; private set; }

	// --- Signals ---
	[Signal]
	public delegate void OnMainNodeSetupFinishedSignalEventHandler();

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.Print($"Duplicate Global instance detected ({Name}). Destroying self.");
			QueueFree();
			return;
		}

		Instance = this;

		GD.Randomize();

		Viewport root = GetTree().Root;
		CurrentSceneNode = root.GetChild(root.GetChildCount() - 1);
		GD.Print($"Global Ready. Initial Scene: {CurrentSceneNode?.Name ?? "None"}");

		// TODO: Initialize UserDataManagementComponent if needed
		// TODO: Load language preference
	}

	// --- Public Methods ---
	public SpellingRuleWordResource GetNextSpellingRuleWordResource() => _wordServerManager?.GetNextSpellingRuleWord();
	public DiactricalMarkWordResource GetNextDiactricalMarkRuleWordResource() => _wordServerManager?.GetNextDiactricalMarkWord();
	public GuessBlockWordResource GetNextGuessBlockWordResource() => _wordServerManager?.GetNextGuessBlockWordResource();

	public void SettingMainNodeData(Player player, StageBase stage)
	{
		if (!IsInstanceValid(player) || !IsInstanceValid(stage))
		{
			GD.PrintErr($"Attempted to set main node data with invalid Player or Stage instance in Global.");
			return;
		}

		// Avoid setting if already set to the same instances (optional optimization)
		if (Player == player && Scene == stage)
		{
			return;
		}

		Player = player;
		Scene = stage;
		GD.Print($"Global main node data set: Player={Player.Name}, Stage={Scene.Name}");

		// Emit signal deferred to ensure listeners in the new scene are ready
		CallDeferred(MethodName.EmitSignal, SignalName.OnMainNodeSetupFinishedSignal);
	}

	public void ClearMainNodeData()
	{
		if (Player != null || Scene != null) // Only print if actually clearing something
		{
			GD.Print("Clearing Global main node data (Player/Scene references).");
			Player = null;
			Scene = null;
		}
	}

	public void ResetGameState() => GD.Print("Resetting global game state (WordServer, DataResources).");
	
	// --- Scene Switching ---
	public void SwitchToMainMenu() => GotoScene("res://UI/Menus/main_menu.tscn");
	public void SwitchToStartGame()
	{
		ResetGameState();
		GotoScene("res://start_game.tscn");
	}

	private void GotoScene(string path)
	{
		// Clear references if Player or Scene currently holds a value,
		// as we are leaving the context where they were valid.
		if (Player != null || Scene != null)
		{
			ClearMainNodeData();
		}

		CallDeferred(MethodName.DeferredGotoScene, path);
	}

	private void DeferredGotoScene(string path)
	{
		GD.Print($"DeferredGotoScene: Changing to '{path}'...");

		if (IsInstanceValid(CurrentSceneNode))
		{
			// Disconnect any signals CurrentSceneNode might have connected TO Global? Unlikely needed here.
			CurrentSceneNode.QueueFree();
			GD.Print($"DeferredGotoScene: Queued freeing of '{CurrentSceneNode.Name}'.");
		}
		else { GD.Print($"DeferredGotoScene: CurrentSceneNode was already invalid."); }

		CurrentSceneNode = null;

		PackedScene nextSceneRes = GD.Load<PackedScene>(path);
		if (nextSceneRes == null)
		{
			GD.PrintErr($"DeferredGotoScene: Failed to load scene resource at path: {path}");
			// TODO: Handle error - maybe load main menu?
			return;
		}

		CurrentSceneNode = nextSceneRes.Instantiate();
		if (CurrentSceneNode == null)
		{
			GD.PrintErr($"DeferredGotoScene: Failed to instantiate scene from resource: {path}");
			return;
		}

		GetTree().Root.AddChild(CurrentSceneNode);
		GD.Print($"DeferredGotoScene: Added new scene '{CurrentSceneNode.Name}' to root.");
		GetTree().CurrentScene = CurrentSceneNode;
		GD.Print($"DeferredGotoScene: Set '{CurrentSceneNode.Name}' as current scene.");
	}
}

// --- Assumed Supporting Code (Place in appropriate files) ---
/* (Assumed code remains the same) */
