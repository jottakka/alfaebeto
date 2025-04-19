using Alfaebeto;
using AlfaEBetto.Components;
using Godot;
using WordProcessing.Enums; // Assuming SupportedLanguage is here

// Assuming Global class is accessible, potentially needs:
// using Alfaebeto;

namespace AlfaEBetto.UI.Menus; // Example corrected/consistent namespace

/// <summary>
/// Controller for the main menu UI scene. Handles button presses for starting
/// the game, opening the store, viewing rules, and sets the initial language.
/// </summary>
public sealed partial class MainMenuUi : Node // Or Control if layout needed at root
{
	#region Exports
	[Export] public Button StartButton { get; set; }
	[Export] public Button StoreButton { get; set; }
	[Export] public Button RulesButton { get; set; }
	[Export] public UiComponent UiComponent { get; set; } // Handles opening other UI panels

	/// <summary>
	/// The language to set globally when this menu loads.
	/// Consider moving language selection to user settings later.
	/// </summary>
	[Export] public SupportedLanguage CurrentLanguage { get; set; } = SupportedLanguage.German; // Default
	#endregion

	#region Private Fields
	private Global _cachedGlobal; // Cache Global instance
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	public override void _ExitTree() => DisconnectSignals();
	#endregion

	#region Initialization & Validation
	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// Ensure menu stays interactive if game pauses
		ProcessMode = ProcessModeEnum.Always;

		// 1. Validate Exports
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes/components. Menu may not function correctly.");
			_isInitialized = false;
			// Optionally deactivate fully: SetProcess(false);
			return;
		}

		// 2. Cache Global Instance
		_cachedGlobal = Global.Instance;
		if (!IsInstanceValid(_cachedGlobal))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Global singleton instance is not valid. Menu cannot function.");
			_isInitialized = false;
			SetProcess(false);
			return;
		}

		// 3. Set Global Language (Consider if this is the final mechanism)
		// This sets the global language based on the value chosen in the Inspector for THIS menu scene.
		// Usually, language is loaded from saved settings.
		_cachedGlobal.CurrentLanguage = CurrentLanguage;
		GD.Print($"{Name}: Set Global Language to {CurrentLanguage}");

		// 4. Connect Signals
		ConnectSignals();

		_isInitialized = true;
	}

	private bool ValidateExports()
	{
		bool isValid = true;
		if (StartButton == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(StartButton)} export."); isValid = false; }

		if (StoreButton == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(StoreButton)} export."); isValid = false; }

		if (RulesButton == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(RulesButton)} export."); isValid = false; }

		if (UiComponent == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(UiComponent)} export."); isValid = false; }

		return isValid;
	}
	#endregion

	#region Signal Handling
	private void ConnectSignals()
	{
		// Use named methods for clarity and easier disconnection
		// Validation ensures buttons are not null here
		StartButton.Pressed += OnStartButtonPressed;
		StoreButton.Pressed += OnStoreButtonPressed;
		RulesButton.Pressed += OnRulesButtonPressed;
	}

	private void DisconnectSignals()
	{
		// Disconnect signals using the same named methods
		if (IsInstanceValid(StartButton))
		{
			StartButton.Pressed -= OnStartButtonPressed;
		}

		if (IsInstanceValid(StoreButton))
		{
			StoreButton.Pressed -= OnStoreButtonPressed;
		}

		if (IsInstanceValid(RulesButton))
		{
			RulesButton.Pressed -= OnRulesButtonPressed;
		}
	}

	// --- Button Press Handlers ---

	private void OnStartButtonPressed()
	{
		if (!IsInstanceValid(_cachedGlobal))
		{
			return; // Extra safety check
		}
		// GD.Print("Start Button Pressed");
		_cachedGlobal.SwitchToStartGame(); // Assumes this method exists on Global
	}

	private void OnStoreButtonPressed()
	{
		if (!IsInstanceValid(UiComponent))
		{
			return; // Extra safety check
		}
		// GD.Print("Store Button Pressed");
		UiComponent.OpenRuleStoreUi(); // Assumes this method exists
	}

	private void OnRulesButtonPressed()
	{
		if (!IsInstanceValid(UiComponent))
		{
			return; // Extra safety check
		}
		// GD.Print("Rules Button Pressed");
		UiComponent.OpenRuleSetsViewingUi(); // Assumes this method exists
	}
	#endregion
}