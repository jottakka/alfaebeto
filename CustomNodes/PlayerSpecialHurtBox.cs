using AlfaEBetto.Extensions;
using Godot;
// Assuming CollisionLayers enum is accessible, likely via:
// using Alfaebeto.Consts;

namespace AlfaEBetto.CustomNodes; // Corrected namespace, assuming this is where it belongs

/// <summary>
/// An Area2D representing a specific hurtbox on the player, potentially for
/// detecting special types of enemy attacks or interactions.
/// Primarily responsible for setting its own collision layer.
/// </summary>
public sealed partial class PlayerSpecialHurtBox : Area2D
{
	// No exports needed, but requires a CollisionShape2D child in the scene.

	public override void _Ready()
	{
		// 1. Validate that a CollisionShape2D child exists
		if (!HasRequiredChildShape())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required CollisionShape2D child node. Area will not function.");
			// Deactivate if shape is missing
			Monitoring = false;
			Monitorable = false;
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		// 2. Set the correct collision layer and clear any mask
		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.PlayerSpecialHurtBox);

		// Note: CollisionMask remains 0. This hurtbox doesn't need to *detect* other areas;
		// other areas (like specific enemy attacks Area2D) need to have
		// PlayerSpecialHurtBox in their *mask* to detect hitting this area.
	}

	/// <summary>
	/// Checks if this Area2D has at least one enabled CollisionShape2D child.
	/// </summary>
	private bool HasRequiredChildShape()
	{
		foreach (Node child in GetChildren())
		{
			// Check if child is CollisionShape2D and not disabled
			if (child is CollisionShape2D shape && !shape.Disabled)
			{
				// Optional: Check if shape resource itself is assigned
				// if (shape.Shape != null) return true;
				return true; // Found at least one enabled shape
			}
		}

		return false; // No enabled CollisionShape2D child found
	}
}