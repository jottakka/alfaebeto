using System; // Required for IDisposable pattern potentially used by GameResultManager
using AlfaEBetto.Consts;
using AlfaEBetto.ManagementNodes;
using AlfaEBetto.PlayerNodes;
using AlfaEBetto.Stages;
using AlfaEBetto.UI;
using Godot;

namespace AlfaEBetto;

// Assuming these exist and are accessible:
// using AlfaEBetto.UI; // For GameOverUi, PauseMenuUi
// using YourProject.Actors; // Player, StageBase
// using YourProject.Global; // Global
// using YourProject.Data; // GameResultManager
// using YourProject.Input; // UserInput constants/actions
// using YourProject.Enums; // GemType

public sealed partial class StartGame : Node2D // Or Node, depending on needs
{
	// --- Exports ---
	[Export] public Player Player { get; set; }
	[Export] public StageBase Stage { get; set; }
	[Export] public GameOverUi GameOverUi { get; set; }
	[Export] public PauseMenuUi PauseMenuUi { get; set; }

	// --- Properties ---
	// GameResultManager handles tracking session stats
	public GameResultManager GameResultManager { get; private set; }

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Cannot start game setup.");
			GetTree()?.Quit(); // Or handle error appropriately
			return;
		}

		// Ensure Global singleton is available
		if (!IsInstanceValid(Global.Instance))
		{
			GD.PrintErr($"{Name}: Global.Instance is not valid in _Ready. Cannot proceed.");
			GetTree()?.Quit(); // Critical error
			return;
		}

		// Register main nodes with Global singleton
		Global.Instance.SettingMainNodeData(Player, Stage);

		// Create and setup GameResultManager
		// Ensure Player is valid before creating manager and connecting signals
		if (IsInstanceValid(Player))
		{
			try
			{
				GameResultManager = new GameResultManager(Player); // Handles internal signal connections
																   // Connect Player death signal locally
				Player.OnPlayerDeathSignal += OnPlayerDeath;
			}
			catch (ArgumentException ex) // Catch potential errors from GameResultManager constructor
			{
				GD.PrintErr($"{Name}: Failed to initialize GameResultManager. Error: {ex.Message}");
				// Handle error - maybe disable score saving?
			}
		}
		else
		{
			GD.PrintErr($"{Name}: Player node is invalid. Cannot initialize GameResultManager or connect death signal.");
			// Handle error - maybe quit?
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Use _Input for single press actions like pause
		// Check if the pause menu itself is valid
		if (IsInstanceValid(PauseMenuUi) && @event.IsActionPressed(UserInput.Pause, true)) // Use exact match 'true'
		{
			// Toggle pause state via the PauseMenuUi
			// Assuming PauseMenuUi handles visibility and pausing the tree internally
			PauseMenuUi.Pause(); // Or maybe a TogglePause() method?

			// --- FIX: Call SetInputAsHandled on the Viewport ---
			GetViewport().SetInputAsHandled(); // Prevent other nodes from processing the pause input
											   // ----------------------------------------------------
		}
	}

	public override void _Process(double delta)
	{
		// _Process is generally used for continuous actions.
		// Pause checking was moved to _Input for better handling.
		// Add any other continuous game logic management here if needed.
	}

	public override void _ExitTree()
	{
		GD.Print($"{Name}: Exiting Tree.");

		// --- CRITICAL: Cleanup connections and resources ---

		// 1. Disconnect local signal handlers
		if (IsInstanceValid(Player))
		{
			Player.OnPlayerDeathSignal -= OnPlayerDeath;
		}

		// 2. Update user data *before* disposing the manager
		// Check validity before calling methods
		if (GameResultManager != null)
		{
			// Check if the manager itself hasn't been disposed prematurely elsewhere
			// (This requires GameResultManager to expose an IsDisposed property or similar,
			// or we rely on the try-catch)
			try
			{
				GameResultManager.UpdateUserData();
			}
			catch (ObjectDisposedException)
			{
				GD.PrintErr($"{Name}: GameResultManager was already disposed before UpdateUserData could be called in _ExitTree.");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"{Name}: Error during GameResultManager.UpdateUserData in _ExitTree: {ex.Message}");
			}

			// 3. Dispose the GameResultManager to disconnect its internal signals
			// This assumes GameResultManager implements IDisposable correctly
			GameResultManager.Dispose();
			GameResultManager = null; // Clear reference
		}

		// 4. Clear global references (optional but good practice)
		// Check if Global instance still exists
		if (IsInstanceValid(Global.Instance))
		{
			// Only clear if the current player/scene are the ones registered
			// This prevents clearing data if a *different* game scene was loaded
			// without this one properly exiting first.
			if (Global.Instance.Player == Player)
			{
				Global.Instance.ClearMainNodeData();
			}
		}
		// -------------------------------------------------
	}

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (Player == null) { GD.PrintErr($"{Name}: Missing Player node!"); isValid = false; }

		if (Stage == null) { GD.PrintErr($"{Name}: Missing Stage node!"); isValid = false; }

		if (GameOverUi == null) { GD.PrintErr($"{Name}: Missing GameOverUi node!"); isValid = false; }

		if (PauseMenuUi == null) { GD.PrintErr($"{Name}: Missing PauseMenuUi node!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Handles the player's death signal. Shows Game Over UI.
	/// </summary>
	private void OnPlayerDeath()
	{
		// Check validity before proceeding
		if (!IsInstanceValid(this))
		{
			return; // Check if this node itself is still valid
		}

		GD.Print($"{Name}: Player death signal received.");

		// Ensure GameOverUI is valid
		if (!IsInstanceValid(GameOverUi))
		{
			GD.PrintErr($"{Name}: GameOverUi is invalid, cannot show game over screen.");
			return;
		}

		// Ensure Tree is valid
		SceneTree tree = GetTree();
		if (tree == null)
		{
			GD.PrintErr($"{Name}: Cannot pause tree or show GameOverUI - Tree is null.");
			return;
		}

		// Pause game and show Game Over screen
		tree.Paused = true;
		GameOverUi.Open(); // Assuming this method makes it visible and plays intro anim
	}
}
