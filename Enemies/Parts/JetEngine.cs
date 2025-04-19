using AlfaEBetto.Enemies.Parts;
using Godot;
// Assuming EnemyPartAnimations constant class exists
// using Alfaebeto.Enemies.Parts;

namespace Alfaebeto.Enemies.Parts; // Corrected namespace

/// <summary>
/// Represents a visual jet engine component, primarily playing an animation.
/// Changed base to Node2D assuming no area detection is needed.
/// </summary>
public sealed partial class JetEngine : Node2D // Changed from Area2D
{
	/// <summary>
	/// The AnimationPlayer node responsible for the engine's visual animation.
	/// Must be assigned in the Inspector.
	/// </summary>
	[Export] public AnimationPlayer AnimationPlayer { get; set; } // Renamed export

	public override void _Ready()
	{
		// 1. Validate Export
		if (AnimationPlayer == null)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Exported node '{nameof(AnimationPlayer)}' is not assigned. Cannot play animation.");
			return;
		}

		// 2. Validate Animation Name (Optional but recommended)
		StringName animName = EnemyPartAnimations.JetEngineMoving; // Assuming this constant exists
		if (!AnimationPlayer.HasAnimation(animName))
		{
			GD.PrintErr($"{Name} ({GetPath()}): AnimationPlayer does not contain animation '{animName}'.");
			return;
		}

		// 3. Play Animation
		AnimationPlayer.Play(animName);
	}

	// No signals connected currently, so _ExitTree for disconnection is not needed yet.
}