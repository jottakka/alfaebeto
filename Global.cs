using AlfaEBetto.Components;
using AlfaEBetto.Data;
using AlfaEBetto.Data.Rules;
using AlfaEBetto.Data.Words;
using AlfaEBetto.ManagementNodes;
using AlfaEBetto.PlayerNodes;
using AlfaEBetto.Stages;
using Godot;
using WordProcessing.Enums;

namespace Alfaebeto; // Corrected namespace

/// <summary>
/// Global singleton node providing access to core managers, game state,
/// data resources, and scene management functionality.
/// </summary>
public partial class Global : Node
{
	#region Singleton Instance
	/// <summary>Gets the singleton instance of the Global node.</summary>
	public static Global Instance { get; private set; }
	#endregion

	#region Components & Managers
	/// <summary>Component handling user data saving/loading (assign or initialize).</summary>
	[Export] // Make it an export if assigned in the editor scene
	public UserDataManagementComponent UserDataManagementComponent { get; private set; } // Made set private

	// Consider initializing these lazily or checking for errors
	private readonly WordServerManager _wordServerManager = new();
	private readonly DataResourceManager _dataResourceManager = new();
	#endregion

	#region Data Resource Accessors (Add null checks for safety)
	public UserDataInfoResource UserDataInfoResource => _dataResourceManager?.UserDataInfoResource;
	public RulesResource RulesResource => _dataResourceManager?.RulesResource;
	public DiactricalMarkWordsDataResource DiactricalMarkWordsDataResource => _dataResourceManager?.DiactricalMarkWordsDataResource;
	public GuessBlockWordsDataResource GuessBlocksDataResource => _dataResourceManager?.GuessBlocksWordsDataResource;
	public SpellingRulesResource SpellingRulesResource => _dataResourceManager?.SpellingRulesResource;
	#endregion

	#region Game State
	/// <summary>Gets or sets the currently active language for gameplay.</summary>
	public SupportedLanguage CurrentLanguage { get; set; } = SupportedLanguage.Japanese; // Default

	/// <summary>Gets the active Player node instance (valid during gameplay scenes).</summary>
	public Player Player { get; private set; }

	/// <summary>Gets the active StageBase node instance (valid during gameplay scenes).</summary>
	public StageBase Stage { get; private set; } // Renamed from Scene to avoid conflict

	/// <summary>Gets the root node of the currently loaded scene.</summary>
	public Node CurrentSceneNode { get; private set; }
	#endregion

	#region Signals
	/// <summary>
	/// Emitted after a new main gameplay scene is loaded AND the Player and StageBase
	/// references within Global have been set. UI elements should wait for this.
	/// </summary>
	[Signal] public delegate void OnMainNodeSetupFinishedSignalEventHandler();
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		// Singleton Enforcement
		if (IsInstanceValid(Instance)) // Use IsInstanceValid for Godot objects
		{
			GD.PrintRich($"[color=orange]Duplicate Global instance detected ({Name}). Destroying self.[/color]");
			QueueFree();
			return;
		}
		Instance = this;
		// Optional: Make truly persistent across scene loads if not already an Autoload
		// ProcessMode = ProcessModeEnum.Always;

		GD.Randomize();

		// Initialize UserDataManagementComponent if it's a child node
		if (UserDataManagementComponent == null)
		{
			UserDataManagementComponent = GetNodeOrNull<UserDataManagementComponent>("UserDataManagementComponent"); // Adjust path if needed
			if (UserDataManagementComponent == null)
			{
				GD.PushWarning($"{Name}: UserDataManagementComponent not found or assigned. User data features may fail.");
				// Optionally create instance: UserDataManagementComponent = new UserDataManagementComponent(); AddChild(UserDataManagementComponent);
			}
		}

		// Get initial scene (likely the one running when Global autoloads)
		CurrentSceneNode = GetTree()?.CurrentScene;
		GD.Print($"Global Ready. Initial Scene: {CurrentSceneNode?.Name ?? "None"}");

		// TODO: Load saved language preference from UserDataManagementComponent
		// CurrentLanguage = UserDataManagementComponent?.LoadLanguagePreference() ?? SupportedLanguage.Japanese;
	}
	#endregion

	#region Public Methods (Data Access)
	public SpellingRuleWordResource GetNextSpellingRuleWordResource() => _wordServerManager?.GetNextSpellingRuleWord();
	public DiactricalMarkWordResource GetNextDiactricalMarkRuleWordResource() => _wordServerManager?.GetNextDiactricalMarkWord();
	public GuessBlockWordResource GetNextGuessBlockWordResource() => _wordServerManager?.GetNextGuessBlockWordResource();
	#endregion

	#region Public Methods (State Management)
	/// <summary>
	/// Sets the global references to the active Player and Stage nodes.
	/// Called internally after a new scene is loaded or explicitly by the Stage itself.
	/// Emits the OnMainNodeSetupFinishedSignal *after* references are set.
	/// </summary>
	/// <param name="player">The active Player node.</param>
	/// <param name="stage">The active StageBase node.</param>
	public void SettingMainNodeData(Player player, StageBase stage)
	{
		// Validate inputs
		if (!IsInstanceValid(player) || !IsInstanceValid(stage))
		{
			GD.PrintErr($"Attempted to set main node data with invalid Player ('{player?.Name ?? "null"}') or Stage ('{stage?.Name ?? "null"}') instance in Global.");
			return;
		}

		// Check if references actually changed
		bool referencesChanged = Player != player || Stage != stage;

		Player = player;
		Stage = stage; // Renamed Scene property to Stage

		if (referencesChanged) // Log only if changed
		{
			GD.Print($"Global main node data set: Player={Player.Name}, Stage={Stage.Name}");
		}

		// Emit signal deferred AFTER setting references
		// This ensures listeners access the now-valid Player/Stage references.
		CallDeferred(MethodName.EmitSignal, SignalName.OnMainNodeSetupFinishedSignal);
	}

	/// <summary>
	/// Clears the global references to Player and Stage.
	/// Called automatically before changing scenes.
	/// </summary>
	public void ClearMainNodeData()
	{
		if (Player != null || Stage != null) // Log only if clearing valid references
		{
			GD.Print("Clearing Global main node data (Player/Stage references).");
			Player = null;
			Stage = null;
		}
	}

	/// <summary>
	/// Resets game-specific state managers (WordServer, DataResources).
	/// Called before starting a new game.
	/// </summary>
	public void ResetGameState() => GD.Print("Resetting global game state (WordServer, DataResources).");// TODO: Implement actual reset logic for _wordServerManager and _dataResourceManager if needed// e.g., _wordServerManager.Reset();// e.g., _dataResourceManager.ReloadData(); // Or reset progress within UserDataInfoResource
	#endregion

	#region Scene Switching
	/// <summary>Switches to the main menu scene.</summary>
	public void SwitchToMainMenu() => GotoScene("res://UI/Menus/main_menu.tscn"); // Verify path

	/// <summary>Resets game state and switches to the main game scene.</summary>
	public void SwitchToStartGame()
	{
		ResetGameState();
		GotoScene("res://start_game.tscn"); // Verify path
	}

	/// <summary>
	/// Initiates a deferred scene change to the specified path.
	/// Clears current Player/Stage references before starting the change.
	/// </summary>
	/// <param name="path">The resource path of the scene to load.</param>
	private void GotoScene(string path)
	{
		// Clear references before initiating scene change
		ClearMainNodeData();

		// Defer the actual scene change operation
		CallDeferred(MethodName.DeferredGotoScene, path);
	}

	/// <summary>
	/// Performs the deferred scene loading, instantiation, adding, and setup.
	/// Finds Player/Stage in the new scene and calls SettingMainNodeData.
	/// </summary>
	private void DeferredGotoScene(string path)
	{
		GD.Print($"DeferredGotoScene: Changing to '{path}'...");

		// Free the old scene
		if (IsInstanceValid(CurrentSceneNode))
		{
			GD.Print($"DeferredGotoScene: Queuing free of old scene '{CurrentSceneNode.Name}'.");
			CurrentSceneNode.QueueFree();
		}
		CurrentSceneNode = null; // Clear reference immediately

		// Load the new scene resource
		PackedScene nextSceneRes = ResourceLoader.Load<PackedScene>(path); // Use ResourceLoader
		if (nextSceneRes == null)
		{
			GD.PrintErr($"DeferredGotoScene: Failed to load scene resource at path: {path}");
			// TODO: Handle error - maybe load main menu as fallback?
			// SwitchToMainMenu(); // Be careful of infinite loops
			return;
		}

		// Instantiate the new scene
		CurrentSceneNode = nextSceneRes.Instantiate();
		if (CurrentSceneNode == null)
		{
			GD.PrintErr($"DeferredGotoScene: Failed to instantiate scene from resource: {path}");
			return;
		}

		// Add the new scene to the tree root and set as current
		GetTree().Root.AddChild(CurrentSceneNode);
		GetTree().CurrentScene = CurrentSceneNode; // Important for GetTree().CurrentScene to work immediately
		GD.Print($"DeferredGotoScene: Added '{CurrentSceneNode.Name}' and set as current scene.");


		// --- *** ADDED LOGIC: Find Player/Stage and Set Data *** ---
		Player foundPlayer = null;
		StageBase foundStage = null;

		// Find Player - Adjust search method as needed (unique name, group, type)
		// Using unique name "%Player" is often robust if set in the scene editor
		foundPlayer = CurrentSceneNode.GetNodeOrNull<Player>("%Player");
		// Fallback search if unique name not used
		foundPlayer ??= CurrentSceneNode.FindChild("Player", recursive: true, owned: false) as Player;

		// Find Stage - Often the root node of the loaded scene itself
		foundStage = CurrentSceneNode as StageBase;
		// Maybe it's a child? Adjust if necessary
		foundStage ??= CurrentSceneNode.GetNodeOrNull<StageBase>("."); // Check root first

		// Check if BOTH were found successfully in the new scene
		if (IsInstanceValid(foundPlayer) && IsInstanceValid(foundStage))
		{
			// Now set the global references AND emit the signal
			SettingMainNodeData(foundPlayer, foundStage); // This now emits the signal AFTER setting Player/Stage
		}
		else
		{
			// Log an error if critical nodes are missing in the loaded gameplay scene
			GD.PrintErr($"DeferredGotoScene: Scene '{path}' loaded, but failed to find required Player ({foundPlayer?.Name ?? "Not Found"}) or StageBase ({foundStage?.Name ?? "Not Found"}) node. OnMainNodeSetupFinishedSignal will not emit.");
		}
		// --- *** END OF ADDED LOGIC *** ---
	}
	#endregion
}
