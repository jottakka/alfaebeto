using Alfaebeto.EnemyWeapons;
using Godot;

namespace Alfaebeto.Enemies.Parts; // Corrected namespace

/// <summary>
/// Represents a wing structure that holds and controls a TurretBase.
/// It relays shooting permissions and provides access to its visibility notifier.
/// Changed base to Node2D assuming no area detection is needed for the wing itself.
/// </summary>
public sealed partial class TurretWing : Node2D // Changed base class
{
	#region Exports
	/// <summary>
	/// Notifier used to detect when the wing (and potentially the turret) goes off-screen.
	/// Accessed by parent nodes (like BaseGuessEnemy) for logic. Assign in Inspector.
	/// </summary>
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }

	/// <summary>
	/// The actual Turret node controlled by this wing. Assign in Inspector.
	/// </summary>
	[Export] public TurretBase Turret { get; set; }
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes. TurretWing may not function correctly.");
			// Optionally deactivate or prevent further processing
			SetProcess(false);
			SetPhysicsProcess(false);
		}
	}
	#endregion

	#region Public Control Methods
	/// <summary>
	/// Allows the associated Turret to shoot.
	/// </summary>
	public void AllowShoot()
	{
		// Check if Turret is valid before calling its method
		if (IsInstanceValid(Turret))
		{
			Turret.AllowShoot();
		}
		else
		{
			GD.PrintErr($"{Name} ({GetPath()}): Cannot call AllowShoot, Turret node is null or invalid.");
		}
	}

	/// <summary>
	/// Prevents the associated Turret from shooting.
	/// </summary>
	public void DisallowShoot() // Corrected typo: Desallow -> Disallow
	{
		// Check if Turret is valid before calling its method
		if (IsInstanceValid(Turret))
		{
			Turret.DisallowShoot();
		}
		else
		{
			GD.PrintErr($"{Name} ({GetPath()}): Cannot call DisallowShoot, Turret node is null or invalid.");
		}
	}
	#endregion

	#region Validation
	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (VisibleOnScreenNotifier2D == null)
		{
			// This might be critical if parent logic relies on it
			GD.PrintErr($"{Name} ({GetPath()}): Exported node '{nameof(VisibleOnScreenNotifier2D)}' is not assigned.");
			isValid = false;
		}

		if (Turret == null)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Exported node '{nameof(Turret)}' is not assigned.");
			isValid = false;
		}

		return isValid;
	}
	#endregion
}
