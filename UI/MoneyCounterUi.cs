using Alfaebeto;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.UI; // Corrected namespace

/// <summary>
/// UI Element responsible for displaying the player's current money count.
/// Updates when the player's money changes via signals.
/// </summary>
public sealed partial class MoneyCounterUi : Control
{
	#region Exports
	/// <summary>Assign the Label node used to display the money count.</summary>
	[Export] public Label MoneyLabel { get; set; } // Renamed for clarity

	/// <summary>Assign the AnimationPlayer used for feedback animations.</summary>
	[Export] public AnimationPlayer AnimationPlayerNode { get; set; } // Renamed for clarity
	
	[Export] public long Money { get; set; } = 0;
	#endregion

	#region Private Fields
	private Player _cachedPlayer;
	private Global _cachedGlobal;
	private bool _isPlayerSignalConnected = false;
	private bool _isGlobalSignalConnected = false;
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	public override void _ExitTree() => DisconnectSignals();
	#endregion

	#region Initialization and Validation
	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// 1. Validate Exports
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required Label or AnimationPlayer export. UI may not function correctly.");
			_isInitialized = false;
			SetProcess(false); SetPhysicsProcess(false);
			return;
		}

		// 2. Initial State & Appearance
		UpdateDisplay(0); // Display initial zero value correctly formatted
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);

		// 3. Get Global Instance
		_cachedGlobal = Global.Instance;
		if (!IsInstanceValid(_cachedGlobal))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Global singleton instance is not valid. Cannot connect setup signal.");
			_isInitialized = false;
			return;
		}

		// 4. Connect to Global Signal (to know when Player is ready)
		_cachedGlobal.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
		_isGlobalSignalConnected = true;

		// 5. Attempt immediate setup if Player might already be ready
		TrySetupPlayerConnection();

		_isInitialized = true;
	}

	private bool ValidateExports()
	{
		bool isValid = true;
		if (MoneyLabel == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(MoneyLabel)} export."); isValid = false; }

		if (AnimationPlayerNode == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(AnimationPlayerNode)} export."); isValid = false; }

		return isValid;
	}
	#endregion

	#region Signal Handling & State Update
	private void ConnectSignals()
	{
		// Handled in Initialize and OnMainNodeReady
	}

	private void DisconnectSignals()
	{
		// Disconnect from Global
		if (_isGlobalSignalConnected && IsInstanceValid(_cachedGlobal))
		{
			_cachedGlobal.OnMainNodeSetupFinishedSignal -= OnMainNodeReady;
		}

		_isGlobalSignalConnected = false;

		// Disconnect from Player
		if (_isPlayerSignalConnected && IsInstanceValid(_cachedPlayer))
		{
			// Assuming Player signal exists and matches delegate
			_cachedPlayer.OnMoneyChangedSignal -= UpdateDisplay; // Disconnect the updated handler
		}

		_isPlayerSignalConnected = false;
	}

	/// <summary>
	/// Called when the Global node signals that the main scene (including the player) is ready.
	/// </summary>
	private void OnMainNodeReady() => TrySetupPlayerConnection();

	/// <summary>
	/// Attempts to cache the player reference, connect to its signals, and update UI.
	/// </summary>
	private void TrySetupPlayerConnection()
	{
		// Only proceed if not already connected and Global exists
		if (_isPlayerSignalConnected || !IsInstanceValid(_cachedGlobal))
		{
			return;
		}

		// Cache player reference *now*
		_cachedPlayer = _cachedGlobal.Player;

		if (IsInstanceValid(_cachedPlayer))
		{
			// Connect to player's signal
			// ASSUMPTION: Player.OnMoneyChangedSignal emits a single 'long' argument: the new total money.
			// You MUST ensure Player.cs emits the signal like this:
			// EmitSignal(SignalName.OnMoneyChangedSignal, _currentMoneyValue);
			_cachedPlayer.OnMoneyChangedSignal -= UpdateDisplay; // Remove first for safety
			_cachedPlayer.OnMoneyChangedSignal += UpdateDisplay; // Connect the new handler
			_isPlayerSignalConnected = true;

			UpdateDisplay(Money);
		}
		else
		{
			GD.PrintErr($"{Name}: OnMainNodeReady called, but Player instance is still not valid.");
		}
	}

	/// <summary>
	/// Updates the displayed money count and plays an animation.
	/// Expected to be called by the Player's OnMoneyChangedSignal.
	/// </summary>
	/// <param name="newTotalMoney">The player's new total money amount.</param>
	public void UpdateDisplay(long newTotalMoney) // Renamed parameter, changed logic
	{
		// Do NOT use internal state like Money += money;
		// Directly display the value received from the player signal or initial setup.

		if (!IsInstanceValid(MoneyLabel))
		{
			return; // Safety check
		}

		// Format the total money for display.
		// Choose a format suitable for 'long'. "N0" adds separators, "D10" pads with zeros.
		// If you need decimals, use 'decimal' or 'double' for money values instead of 'long'.
		MoneyLabel.Text = $"$ {newTotalMoney:N0}"; // Example: "$ 1,234"
												   // Or for zero padding (e.g., 10 digits):
												   // MoneyLabel.Text = $"$ {newTotalMoney:D10}"; // Example: "$ 0000012345"

		// Play feedback animation safely
		// Ensure constant name matches definition (e.g., in a static UiAnimations class)
		if (IsInstanceValid(AnimationPlayerNode))
		{
			if (AnimationPlayerNode.HasAnimation(UiAnimations.OnAddMoney))
			{
				AnimationPlayerNode.Play(UiAnimations.OnAddMoney);
			}
			else
			{
				GD.PushWarning($"{Name}: Animation '{UiAnimations.OnAddMoney}' not found in AnimationPlayer.");
			}
		}
	}
	#endregion
}