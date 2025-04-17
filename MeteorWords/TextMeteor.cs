using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.MeteorWords;

public sealed partial class TextMeteor : Area2D
{
	// --- Exports ---
	// Assuming these are Labels used to display parts of a word/question
	[Export] public Label WordFirstPart { get; set; }
	[Export] public Label WordLastPart { get; set; }
	[Export] public Label QuestionMarkLabel { get; set; } // For the part to be guessed?
	[Export] public AnimationPlayer AnimationPlayer { get; set; }

	// --- Signals ---
	// Emitted after the death animation finishes, indicating the parent can queue_free this node.
	[Signal] public delegate void ReadyToQueueFreeSignalEventHandler();

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. TextMeteor may not function correctly.");
			// Consider SetProcess(false) or other error state?
			return;
		}

		// Connect signals using strongly-typed +=
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _ExitTree()
	{
		// Disconnect signals when removed from the tree
		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}
	}

	// --- Public Methods ---

	/// <summary>
	/// Starts the destruction sequence for this meteor.
	/// Plays different animations based on whether the correct target was hit.
	/// </summary>
	/// <param name="wasTargetDestroyed">True if the associated correct answer meteor was destroyed.</param>
	public void Destroy(bool wasTargetDestroyed)
	{
		// Assuming ResetCollisionLayerAndMask exists and is needed for Area2D
		this.ResetCollisionLayerAndMask(); // Disable collision detection

		// Choose animation based on success
		string animToPlay = wasTargetDestroyed
			? MeteorAnimations.TextMeteorDeathTargetHit
			: MeteorAnimations.TextMeteorDeathTargetNotHit;

		// Play animation (check validity)
		AnimationPlayer?.Play(animToPlay);

		// If no animation player, maybe emit signal immediately? Or just hide?
		if (AnimationPlayer == null)
		{
			GD.PrintErr($"{Name}: No AnimationPlayer found in Destroy. Emitting ReadyToQueueFreeSignal immediately.");
			EmitSignal(SignalName.ReadyToQueueFreeSignal);
			// Optionally hide: Hide();
		}
	}

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		// These labels might be optional depending on the game mode (e.g., Japanese vs German)
		// Add checks only if they are always required.
		// if (WordFirstPart == null) { GD.PrintErr($"{Name}: Missing WordFirstPart Label!"); isValid = false; }
		// if (WordLastPart == null) { GD.PrintErr($"{Name}: Missing WordLastPart Label!"); isValid = false; }
		// if (QuestionMarkLabel == null) { GD.PrintErr($"{Name}: Missing QuestionMarkLabel Label!"); isValid = false; }
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Handles the AnimationFinished signal from the AnimationPlayer.
	/// Emits ReadyToQueueFreeSignal after death animations complete.
	/// </summary>
	private void OnAnimationFinished(StringName animationName)
	{
		// Check if this instance is still valid
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Check if the finished animation was one of the death animations
		if (animationName == MeteorAnimations.TextMeteorDeathTargetHit ||
			animationName == MeteorAnimations.TextMeteorDeathTargetNotHit)
		{
			// Use SignalName for type safety (Godot 4+)
			EmitSignal(SignalName.ReadyToQueueFreeSignal);
		}
		// Handle other animation finishes if needed
	}
}
