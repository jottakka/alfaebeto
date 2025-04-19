using Alfaebeto;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;
// Assuming Global and VisibilityZOrdering are accessible
// using Alfaebeto;
// using Alfaebeto.Consts;

namespace AlfaEBetto.UI; // Corrected namespace

/// <summary>
/// UI Element responsible for displaying the player's collected gem counts.
/// Updates automatically when the player collects gems via signals.
/// </summary>
public sealed partial class GemsUi : MarginContainer
{
	#region Exports
	[Export] public Label GreenGemLabel { get; set; }
	[Export] public Label RedGemLabel { get; set; }
	#endregion

	#region Private Fields
	// Internal state - consider if this should read directly from player instead
	// Keeping internal state for now as per original design.
	private int _greenGems = 0;
	private int _redGems = 0;

	private Player _cachedPlayer; // Cache player reference safely
	private Global _cachedGlobal; // Cache global instance

	// Track signal connection status for safe disconnection
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
			GD.PrintErr($"{Name} ({GetPath()}): Missing required Label exports. UI will not display correctly.");
			_isInitialized = false; // Prevent further setup
			SetProcess(false); SetPhysicsProcess(false);
			return;
		}

		// 2. Initial State & Appearance
		GreenGemLabel.Text = GetFormattedCount(_greenGems); // Use helper immediately
		RedGemLabel.Text = GetFormattedCount(_redGems);
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);

		// 3. Get Global Instance
		_cachedGlobal = Global.Instance;
		if (!IsInstanceValid(_cachedGlobal))
		{
			GD.PrintErr($"{Name} ({GetPath()}): Global singleton instance is not valid. Cannot connect setup signal.");
			_isInitialized = false; // Cannot proceed without Global
			return;
		}

		// 4. Connect to Global Signal (to know when Player is ready)
		// Check if already connected only needed if Initialize could run multiple times
		_cachedGlobal.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
		_isGlobalSignalConnected = true;

		// 5. Attempt immediate setup if Player might already be ready
		// This handles cases where UI loads *after* player setup finishes
		TrySetupPlayerConnection();

		_isInitialized = true; // Mark basic setup as done
	}

	private bool ValidateExports()
	{
		bool isValid = true;
		if (GreenGemLabel == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(GreenGemLabel)} export."); isValid = false; }

		if (RedGemLabel == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(RedGemLabel)} export."); isValid = false; }

		return isValid;
	}
	#endregion

	#region Signal Handling & State Update
	private void ConnectSignals()
	{
		// Connections are handled in Initialize and OnMainNodeReady
		// Disconnection logic is in DisconnectSignals
	}

	private void DisconnectSignals()
	{
		// Disconnect from Global singleton
		if (_isGlobalSignalConnected && IsInstanceValid(_cachedGlobal))
		{
			_cachedGlobal.OnMainNodeSetupFinishedSignal -= OnMainNodeReady;
		}

		_isGlobalSignalConnected = false; // Ensure flag is reset

		// Disconnect from Player
		if (_isPlayerSignalConnected && IsInstanceValid(_cachedPlayer))
		{
			// Check if signal exists and is connected if necessary/possible
			_cachedPlayer.OnGemAddedSignal -= OnGemsChanged;
		}

		_isPlayerSignalConnected = false; // Ensure flag is reset
	}

	/// <summary>
	/// Called when the Global node signals that the main scene (including the player) is ready.
	/// </summary>
	private void OnMainNodeReady() =>
		// Now that we know player *should* exist, try to connect
		TrySetupPlayerConnection();

	/// <summary>
	/// Attempts to cache the player reference, connect to its signals, and update UI.
	/// Safe to call multiple times (e.g., from _Ready and OnMainNodeReady).
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
			// Remove first to prevent potential duplicates if called multiple times
			_cachedPlayer.OnGemAddedSignal -= OnGemsChanged;
			_cachedPlayer.OnGemAddedSignal += OnGemsChanged;
			_isPlayerSignalConnected = true;

			// IMPORTANT: Update UI with current values immediately after connecting
			UpdateGemCount(GemType.Green); // Assuming Player has properties like GreenGems/RedGems
			UpdateGemCount(GemType.Red);
			// GD.Print($"{Name}: Connected to Player signals and updated counts.");
		}
		else
		{
			GD.PrintErr($"{Name}: OnMainNodeReady called, but Player instance is still not valid in Global singleton.");
		}
	}

	/// <summary>
	/// Called when the Player signals that a gem has been added.
	/// </summary>
	private void OnGemsChanged(GemType gemType)
=> UpdateGemCount(gemType);

	/// <summary>
	/// Updates the internal state and label text for a specific gem type.
	/// </summary>
	private void UpdateGemCount(GemType gemType)
	{
		switch (gemType)
		{
			case GemType.Green:
				if (IsInstanceValid(GreenGemLabel)) // Safety check
				{
					_greenGems++;
					GreenGemLabel.Text = GetFormattedCount(_greenGems);
				}

				break;
			case GemType.Red:
				if (IsInstanceValid(RedGemLabel)) // Safety check
				{
					RedGemLabel.Text = GetFormattedCount(_redGems);
					_redGems++;
					RedGemLabel.Text = GetFormattedCount(_redGems);
				}

				break;
		}
	}

	/// <summary>
	/// Formats the integer count as a zero-padded three-digit string.
	/// </summary>
	private string GetFormattedCount(int count) => $"{count:000}";

	// Removed AddGreenGem/AddRedGem as logic is now in UpdateGemCount/OnGemsChanged

	#endregion
}