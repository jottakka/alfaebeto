// Assuming child UI elements are in Alfaebeto.UI
using AlfaEBetto.UI;
using Godot;

namespace Alfaebeto.UI; // Corrected namespace

/// <summary>
/// The main container node for the gameplay UI (HUD).
/// Holds references to different UI segments like health, money, gems.
/// </summary>
public sealed partial class FullUi : Control // Control is a suitable base for UI root
{
	#region Exports
	/// <summary>Assign the MoneyCounterUi node in the Inspector.</summary>
	[Export] public MoneyCounterUi MoneyCounterUi { get; set; }

	/// <summary>Assign the HeartShieldUi node in the Inspector.</summary>
	[Export] public HeartShieldUi HeartShieldUi { get; set; }

	/// <summary>Assign the GemsUi node in the Inspector.</summary>
	[Export] public GemsUi GemsUi { get; set; }
	#endregion

	private bool _isInitialized = false;

	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported UI component references. Full UI may not display correctly.");
			SetProcess(false);
			SetPhysicsProcess(false);
			_isInitialized = false; // Mark as failed?
			return;
		}

		// Add any other initialization logic for the FullUi itself if needed

		_isInitialized = true;
	}

	/// <summary>
	/// Validates that essential exported UI component nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		// Log errors for missing critical references
		if (MoneyCounterUi == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(MoneyCounterUi)} export."); isValid = false; }

		if (HeartShieldUi == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(HeartShieldUi)} export."); isValid = false; }

		if (GemsUi == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing {nameof(GemsUi)} export."); isValid = false; }

		return isValid;
	}
}