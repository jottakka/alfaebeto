using Alfaebeto;
using AlfaEBetto.Components;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.UI;

public sealed partial class PauseMenuUi : Control
{
	// --- Exports ---
	[Export] public UiComponent UiComponent { get; set; } // Assuming this is a custom component/node
	[Export] public Button ContinueButton { get; set; }
	[Export] public Button ExitButton { get; set; }
	[Export] public Button RulesButton { get; set; }

	// --- Properties ---
	// Cache Global instance for slightly cleaner access, check validity on use
	private Global _global => Global.Instance;

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree(); // Cannot function without required nodes
			return;
		}

		Hide(); // Start hidden
				// ProcessModeEnum.Always is correct for a pause menu that needs to be interactive when the game tree is paused.
		ProcessMode = ProcessModeEnum.Always;

		// Assuming SetVisibilityZOrdering extension method exists
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);

		// --- Connect Signals ---

		// Connect to Global singleton signal - MUST be disconnected in _ExitTree
		if (_global != null) // Check if Global exists
		{
			_global.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
		}
		else
		{
			GD.PrintErr($"{Name}: Global.Instance is null in _Ready. Cannot connect setup signal.");
		}

		// Connect button signals directly in Ready (ensures one-time connection)
		RulesButton.Pressed += OnRulesButtonPressed;
		ContinueButton.Pressed += OnContinueButtonPressed;
		ExitButton.Pressed += OnExitButtonPressed;
	}

	public override void _ExitTree()
	{
		// --- CRITICAL: Disconnect all signals connected in _Ready ---

		// Disconnect from Global singleton
		if (IsInstanceValid(_global)) // Check if Global instance is still valid
		{
			_global.OnMainNodeSetupFinishedSignal -= OnMainNodeReady;
		}

		// Disconnect from local buttons (check validity)
		if (IsInstanceValid(RulesButton))
		{
			RulesButton.Pressed -= OnRulesButtonPressed;
		}

		if (IsInstanceValid(ContinueButton))
		{
			ContinueButton.Pressed -= OnContinueButtonPressed;
		}

		if (IsInstanceValid(ExitButton))
		{
			ExitButton.Pressed -= OnExitButtonPressed;
		}
	}

	// --- Public Methods ---

	/// <summary>
	/// Pauses the game and shows the pause menu.
	/// </summary>
	public void Pause()
	{
		// Check if tree is valid before pausing
		if (GetTree() != null)
		{
			GetTree().Paused = true;
			Show();
			// Optionally, bring to front just in case
			MoveToFront();
		}
		else
		{
			GD.PrintErr($"{Name}: Cannot Pause - Tree is null.");
		}
	}

	// --- Signal Handlers ---

	/// <summary>
	/// Handler for when the main game node setup is finished (signal from Global).
	/// Can be used if pause menu needs info available only after main setup.
	/// </summary>
	private void OnMainNodeReady()
	{
		// --- FIX: Check if this instance itself is still valid ---
		if (!IsInstanceValid(this))
		{
			return; // Exit immediately if this instance is disposed
		}
		// ----------------------------------------------------------

		// Button connections are now in _Ready.
		// Add any logic here that *must* run only after the main game scene is fully set up.
		// Example: Fetching player stats for display on the pause menu.
		// Remember to add IsInstanceValid(_global?.Player) checks if accessing player data.
		GD.Print($"{Name}: OnMainNodeReady received.");
	}

	/// <summary>
	/// Handler for the Rules button press.
	/// </summary>
	private void OnRulesButtonPressed()
	{
		if (!IsInstanceValid(this))
		{
			return; // Validity check
		}

		// Check if UiComponent is valid before calling its method
		if (IsInstanceValid(UiComponent))
		{
			UiComponent.OpenRuleSetsViewingUi(); // Assuming this method exists
		}
		else
		{
			GD.PrintErr($"{Name}: UiComponent is invalid. Cannot open rules.");
		}
	}

	/// <summary>
	/// Handler for the Continue button press.
	/// </summary>
	private void OnContinueButtonPressed()
	{
		if (!IsInstanceValid(this))
		{
			return; // Validity check
		}

		// Check if tree is valid before unpausing
		if (GetTree() != null)
		{
			GetTree().Paused = false;
			Hide();
		}
		else
		{
			GD.PrintErr($"{Name}: Cannot Continue - Tree is null.");
		}
	}

	/// <summary>
	/// Handler for the Exit button press.
	/// </summary>
	private void OnExitButtonPressed()
	{
		if (!IsInstanceValid(this))
		{
			return; // Validity check
		}

		// Check if tree and global are valid before proceeding
		SceneTree tree = GetTree();
		Global globalInstance = _global; // Use cached access via property

		if (tree != null)
		{
			tree.Paused = false; // Always unpause before changing scenes
		}
		else
		{
			GD.PrintErr($"{Name}: Tree is null. Cannot unpause.");
		}

		if (IsInstanceValid(globalInstance))
		{
			globalInstance.SwitchToMainMenu();
		}
		else
		{
			GD.PrintErr($"{Name}: Cannot switch to Main Menu - Global instance is invalid.");
			// Fallback?
			// GetTree()?.ChangeSceneToFile("res://UI/Menus/main_menu.tscn");
		}
	}

	// --- Helpers ---

	private bool ValidateExports()
	{
		bool isValid = true;
		if (UiComponent == null) { GD.PrintErr($"{Name}: Missing UiComponent!"); isValid = false; }

		if (ContinueButton == null) { GD.PrintErr($"{Name}: Missing ContinueButton!"); isValid = false; }

		if (ExitButton == null) { GD.PrintErr($"{Name}: Missing ExitButton!"); isValid = false; }

		if (RulesButton == null) { GD.PrintErr($"{Name}: Missing RulesButton!"); isValid = false; }

		return isValid;
	}
}
