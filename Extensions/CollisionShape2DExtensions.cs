using Godot;

// Assuming CollisionLayers enum is defined elsewhere, e.g.:
// using Alfaebeto.Consts;

namespace AlfaEBetto.Extensions; // Corrected namespace

/// <summary>
/// Provides extension methods for Godot's CollisionObject2D class
/// to simplify manipulation of collision layers and masks using a CollisionLayers enum.
/// Assumes the CollisionLayers enum values correspond to the 1-based physics layer indices.
/// </summary>
public static class CollisionObject2DExtensions
{
	private const int MIN_LAYER_INDEX = 1;
	private const int MAX_LAYER_INDEX = 32; // Godot supports 32 collision layers/masks

	/// <summary>
	/// Resets both the collision layer and mask to 0, effectively disabling all collisions
	/// defined by layers/masks.
	/// </summary>
	/// <param name="collisionObject">The collision object to modify.</param>
	public static void ResetCollisionLayerAndMask(this CollisionObject2D collisionObject)
	{
		if (!ValidateObject(collisionObject, nameof(ResetCollisionLayerAndMask)))
		{
			return;
		}

		collisionObject.CollisionLayer = 0;
		collisionObject.CollisionMask = 0;
	}

	/// <summary>
	/// Activates a specific collision layer bit.
	/// Assumes the enum value corresponds to the 1-based layer index.
	/// </summary>
	/// <param name="collisionObject">The collision object to modify.</param>
	/// <param name="layer">The collision layer enum value to activate.</param>
	public static void ActivateCollisionLayer(this CollisionObject2D collisionObject, CollisionLayers layer) => SetLayerOrMaskValue(collisionObject, layer, true, isLayer: true, nameof(ActivateCollisionLayer));

	/// <summary>
	/// Deactivates a specific collision layer bit.
	/// Assumes the enum value corresponds to the 1-based layer index.
	/// </summary>
	/// <param name="collisionObject">The collision object to modify.</param>
	/// <param name="layer">The collision layer enum value to deactivate.</param>
	public static void DeactivateCollisionLayer(this CollisionObject2D collisionObject, CollisionLayers layer) => SetLayerOrMaskValue(collisionObject, layer, false, isLayer: true, nameof(DeactivateCollisionLayer));

	/// <summary>
	/// Activates a specific collision mask bit (i.e., allows collision *with* the specified layer).
	/// Assumes the enum value corresponds to the 1-based layer index.
	/// </summary>
	/// <param name="collisionObject">The collision object to modify.</param>
	/// <param name="layer">The collision layer enum value to activate in the mask.</param>
	public static void ActivateCollisionMask(this CollisionObject2D collisionObject, CollisionLayers layer) => SetLayerOrMaskValue(collisionObject, layer, true, isLayer: false, nameof(ActivateCollisionMask));

	/// <summary>
	/// Deactivates a specific collision mask bit (i.e., prevents collision *with* the specified layer).
	/// Assumes the enum value corresponds to the 1-based layer index.
	/// </summary>
	/// <param name="collisionObject">The collision object to modify.</param>
	/// <param name="layer">The collision layer enum value to deactivate in the mask.</param>
	public static void DeactivateCollisionMask(this CollisionObject2D collisionObject, CollisionLayers layer) => SetLayerOrMaskValue(collisionObject, layer, false, isLayer: false, nameof(DeactivateCollisionMask));

	// --- Helper Methods ---

	/// <summary>
	/// Internal helper to set layer or mask values with validation.
	/// </summary>
	private static void SetLayerOrMaskValue(CollisionObject2D collisionObject, CollisionLayers layerEnumValue, bool enabled, bool isLayer, string callerMethodName)
	{
		if (!ValidateObject(collisionObject, callerMethodName))
		{
			return;
		}

		int layerIndex = (int)layerEnumValue;

		// Validate the derived index
		if (layerIndex is < MIN_LAYER_INDEX or > MAX_LAYER_INDEX)
		{
			// Use GD.PrintErr for runtime errors
			GD.PrintErr($"CollisionObject2DExtensions.{callerMethodName}: Invalid layer index {layerIndex} derived from enum value '{layerEnumValue}'. Index must be between {MIN_LAYER_INDEX} and {MAX_LAYER_INDEX}.");
			return;
		}

		// Set the value
		if (isLayer)
		{
			collisionObject.SetCollisionLayerValue(layerIndex, enabled);
		}
		else
		{
			collisionObject.SetCollisionMaskValue(layerIndex, enabled);
		}
	}

	/// <summary>
	/// Validates if the CollisionObject2D instance is not null.
	/// </summary>
	/// <param name="collisionObject">The object to validate.</param>
	/// <param name="callerMethodName">Name of the public method calling this validation.</param>
	/// <returns>True if valid, false otherwise.</returns>
	private static bool ValidateObject(CollisionObject2D collisionObject, string callerMethodName)
	{
		if (collisionObject == null)
		{
			// Use GD.PrintErr to report misuse of the extension method
			GD.PrintErr($"CollisionObject2DExtensions.{callerMethodName} called on a null CollisionObject2D instance.");
			return false;
		}
		// Optional: Check IsInstanceValid if you suspect it might be called on freed nodes
		// if (!GodotObject.IsInstanceValid(collisionObject)) { ... return false; }
		return true;
	}
}

/* Reminder: Ensure your CollisionLayers enum values match the 1-based indices
       you intend to use in the Godot editor's collision layer settings.
       Example (Ensure this exists, e.g., in Consts/CollisionLayers.cs):
       public enum CollisionLayers
       {
           Default = 1,
           Player = 2,
           Enemy = 3,
           PlayerHitBox = 4,
           EnemyHitBox = 5, // General enemy hurt area
           PlayerCollectionArea = 6,
           PlayerSpecialHurtBox = 7,
           Collectables = 10,
           WordEnemy = 11, // Layer for the block itself
           WordEnemyHitBox = 12, // Layer for the block's hitbox area
           MeteorEnemy = 13,
           MeteorEnemyHurtBox = 14,
           EnemyAmmo = 15,
           PlayerAmmo = 16, // Added example
           PlayerShieldHitBox = 17, // Added example
           RegularEnemyHurtBox = 18, // Added example
           // ... define layers 1 through 32 as needed ...
       }
    */