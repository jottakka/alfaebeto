using Alfaebeto.Blocks;    // For LetterBlock, Word, ArticlesSet (assuming EnemyWord uses Blocks namespace too)
using Alfaebeto.Enemies;
using AlfaEBetto.Ammo;
using AlfaEBetto.Blocks;
using AlfaEBetto.Enemies;
using AlfaEBetto.Extensions;
using AlfaEBetto.MeteorWords;
using Godot;

namespace Alfaebeto.CustomNodes; // Assuming namespace for HitBox/HurtBox

/// <summary>
/// An Area2D representing the area where an enemy can be hurt.
/// Automatically sets its collision layer based on the parent node's type during _Ready.
/// It typically doesn't set its mask, relying on attacking objects (like player ammo)
/// to have the correct mask to detect this hurtbox's layer.
/// </summary>
public sealed partial class EnemyHurtBox : Area2D
{
	// Keep Parent property if frequently accessed, otherwise GetParent() is fine too.
	// public Node ParentNode => GetParent(); // Renamed for clarity

	public override void _Ready()
	{
		// Ensure starting clean
		this.ResetCollisionLayerAndMask();

		// Set the correct layer based on the parent type
		SetCollisionLayerBasedOnParent();
	}

	/// <summary>
	/// Sets the appropriate collision layer for this hurtbox based on its parent node's type.
	/// </summary>
	public void SetCollisionLayerBasedOnParent() // Renamed from ActivateCollisionsMasks
	{
		Node parentNode = GetParent(); // Get parent reference

		if (parentNode == null)
		{
			GD.PrintErr($"{Name}: HurtBox has no parent node. Cannot set collision layer.");
			return;
		}

		// Determine layer based on parent type using pattern matching
		switch (parentNode)
		{
			// Specific Enemy Types first
			case MeteorEnemyBase _:
				SetLayer(CollisionLayers.MeteorEnemyHurtBox);
				break;
			// Group common word/block types (adjust if EnemyWord inherits differently)
			case LetterBlock _ or BlockSetBase _ or AnswerMeteor _ or BaseGuessEnemy:
				// Assuming all these word/block related interactive elements use the same hurtbox layer
				SetLayer(CollisionLayers.WordEnemyHurtBox);
				break;
			// General Enemy Type (Catches types inheriting from EnemyBase but not MeteorEnemyBase)
			case EnemyBase _: // Put this after more specific enemy types
				SetLayer(CollisionLayers.RegularEnemyHurtBox);
				break;
			// Ignore specific types that shouldn't have this hurtbox logic
			case AmmoBase _:
				// Ammo shouldn't typically have an EnemyHurtBox, do nothing.
				GD.PrintRich($"[color=orange]{Name}: EnemyHurtBox attached to AmmoBase parent '{parentNode.Name}'. Hurtbox will not be active.[/color]");
				break;
			// Default case for unrecognized parents
			default:
				GD.PrintErr($"{Name}: HurtBox parent type '{parentNode.GetType().Name}' is not recognized or handled. Hurtbox layer not set.");
				break;
		}
	}

	/// <summary>
	/// Deactivates collisions by resetting the layer and mask.
	/// </summary>
	public void DeactivateCollisions()
	{
		this.ResetCollisionLayerAndMask();
		// Optionally disable the monitoring state as well if no longer needed
		this.Monitoring = false;
	}

	/// <summary>
	/// Helper method to activate a single collision layer.
	/// </summary>
	/// <param name="layer">The layer to activate.</param>
	private void SetLayer(CollisionLayers layer)
	{
		// Assumes ResetCollisionLayerAndMask was called before this
		this.Monitoring = true;

		this.ActivateCollisionLayer(layer);
	}
}