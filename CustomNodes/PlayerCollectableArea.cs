using AlfaEBetto.Extensions;
using Godot;
// Assuming CollisionLayers enum is accessible, likely via:
// using Alfaebeto.Consts;

namespace Alfaebeto.CustomNodes; // Corrected namespace, assuming this is where it belongs

/// <summary>
/// An Area2D defining the region around the player where collectable items
/// become attracted (homing behavior starts).
/// Primarily responsible for setting its own collision layer.
/// </summary>
public sealed partial class PlayerCollectableArea : Area2D
{
	/// <summary>
	/// The CollisionShape2D defining the size and shape of the collection area.
	/// Must be assigned in the Inspector.
	/// </summary>
	[Export] public CollisionShape2D CollisionShape2D { get; set; }

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required CollisionShape2D. Area will not function.");
			// Deactivate or prevent further processing if shape is missing
			Monitoring = false;
			Monitorable = false;
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		// Set the correct collision layer and clear any mask (it doesn't need to detect others)
		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.PlayerCollectionArea);

		// Note: CollisionMask remains 0. This area doesn't need to *detect* other areas;
		// other areas (like CollectableItemBase) need to have PlayerCollectionArea
		// in their *mask* to detect entering this area.
	}

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		if (CollisionShape2D == null)
		{
			// Use GD.PrintErr for errors that prevent functionality
			GD.PrintErr($"{Name} ({GetPath()}): Exported node '{nameof(CollisionShape2D)}' is not assigned.");
			return false;
		}
		// Optional: Check if CollisionShape2D actually has a shape resource assigned
		// if (CollisionShape2D.Shape == null) { ... }
		return true;
	}
}