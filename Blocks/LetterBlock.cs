using Alfaebeto.Components;
using AlfaEBetto.Components;
using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using Godot;
// using Alfaebeto.Blocks; // If LetterBlockAnimations is here

namespace AlfaEBetto.Blocks;

// Consider if StaticBody2D is the best fit. If only hit detection via HitBox (Area2D)
// is needed and it never moves, Node2D might suffice. If physics interaction is needed,
// consider RigidBody2D or CharacterBody2D. Sticking with StaticBody2D for now.
public partial class LetterBlock : StaticBody2D
{
	#region Exports
	// Nodes primarily for visual/label configuration
	[Export] public Sprite2D Sprite { get; set; }
	[Export] public Label Label { get; set; }
	[Export] public Sprite2D DeathSpriteEffect { get; set; }
	[Export] public Sprite2D ExplosionsSprite2D { get; set; }
	[Export] public CollisionShape2D CollisionShape { get; set; } // Needed for StaticBody2D

	// Configuration
	[Export] public bool IsTarget { get; set; } // Is this the correct block to hit?

	// Required Components (Consider assigning in Inspector or ensuring they exist in scene)
	[Export] public HitBox HitBox { get; set; }
	[Export] public HurtComponent HurtComponent { get; set; }
	[Export] public HealthComponent HealthComponent { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; } // Renamed export


	[Export] public int DamageTakenPerHit { get; set; } = 20;

	#endregion

	#region Signals
	// Emitted after the 'TargetDying' animation finishes, before exploding.
	[Signal] public delegate void OnTargetBlockCalledDestructionSignalEventHandler();
	// Emitted immediately when health reaches zero (carries if it was the target).
	[Signal] public delegate void OnLetterDestructedSignalEventHandler(bool isTarget);
	// Emitted just before QueueFree is called after the explosion animation.
	[Signal] public delegate void OnReadyToDequeueSignalEventHandler();
	#endregion

	#region State
	public bool IsDead { get; private set; } = false;
	private int _currentSpriteFrame = 0; // Corrected typo
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required nodes/components. Queuing free.");
			QueueFree();
			return;
		}

		// Initial visual setup
		ExplosionsSprite2D.Frame = GD.RandRange(0, ExplosionsSprite2D.Hframes - 1); // Use Hframes for safety
		Sprite.Frame = _currentSpriteFrame;
		DeathSpriteEffect.Visible = false;

		// Configure components
		SetupHealthComponent(); // Do this before connecting signals that depend on it

		// Connect signals
		ConnectSignals();

		// Configure collision
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionMask(CollisionLayers.Player); // Mask determines what *this* body collides with
	}

	public override void _ExitTree() => DisconnectSignals();
	#endregion

	#region Public API
	public void SetLabel(char letter) => SetLabel(letter.ToString());

	public void SetLabel(string word)
	{
		if (Label != null)
		{
			Label.Text = word;
		}
	}

	public void SetLabelColor(Color color) => Label?.AddThemeColorOverride("font_color", color); // Add null check

	public void SetBlockPosition(Vector2 position) => GlobalPosition = position; // Use GlobalPosition typically

	/// <summary>
	/// Starts the explosion animation sequence directly. Usually called internally or by parent.
	/// </summary>
	public void TriggerExplosion()
	{
		// Ensure required nodes are valid before playing animation
		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.Play(LetterBlockAnimations.OnLetterBlockExplode);
		}
		else
		{
			GD.PrintErr($"{Name}: Cannot TriggerExplosion, AnimationPlayer is invalid.");
			// Fallback: just queue free immediately if animation fails
			EmitSignal(SignalName.OnReadyToDequeueSignal);
			QueueFree();
		}
	}

	/// <summary>
	/// Disables collision detection for this block.
	/// </summary>
	public void DisableCollisions()
	{
		HitBox?.DeactivateCollisions(); // Use null check
		CollisionShape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, true); // Disable StaticBody collision
		this.ResetCollisionLayerAndMask(); // Clear layers/masks as well
	}
	#endregion

	#region Internal Logic & Signal Handlers

	private void SetupHealthComponent()
	{
		if (HealthComponent == null)
		{
			return; // Guard clause
		}

		HealthComponent.EmitInBetweenSignals = true;
		HealthComponent.HealthLevelSignalsIntervals = 3;
		// Connect signals here before they might be emitted
		HealthComponent.OnHealthDepletedSignal += HandleHealthDepleted;
		HealthComponent.OnHealthLevelChangeSignal += OnHealthLevelChanged;
	}

	private void ConnectSignals()
	{
		if (HurtComponent != null)
		{
			HurtComponent.OnHurtSignal += OnHurt;
		}

		if (AnimationPlayer != null)
		{
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
		}

		// Health signals are connected in SetupHealthComponent to ensure correct order
	}

	private void DisconnectSignals()
	{
		// Use IsInstanceValid for safety before accessing potentially freed nodes
		if (IsInstanceValid(HurtComponent))
		{
			HurtComponent.OnHurtSignal -= OnHurt;
		}

		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}

		if (IsInstanceValid(HealthComponent))
		{
			HealthComponent.OnHealthDepletedSignal -= HandleHealthDepleted;
			HealthComponent.OnHealthLevelChangeSignal -= OnHealthLevelChanged;
		}
	}

	private void OnHealthLevelChanged(int healthLevel)
	{
		// Ensure frame exists before setting
		//if (Sprite != null && healthLevel >= 0 && healthLevel < Sprite.Hframes) // Assuming horizontal frames indicate health
		{
			_currentSpriteFrame = healthLevel;
			Sprite.Frame = _currentSpriteFrame;
		}
	}

	private void OnAnimationFinished(StringName animationName)
	{
		// Use StringName comparison for performance
		if (animationName == LetterBlockAnimations.OnLetterBlockExplode)
		{
			EmitSignal(SignalName.OnReadyToDequeueSignal);
			QueueFree(); // Bye bye!
		}
		else if (animationName == LetterBlockAnimations.OnLetterBlockDyingTarget)
		{
			// Target finished its "dying" animation, now tell parent to explode everything
			EmitSignal(SignalName.OnTargetBlockCalledDestructionSignal);
			// Typically, the parent receiving this signal will call TriggerExplosion() on all blocks
		}
		else
		{
			AnimationPlayer.Play(LetterBlockAnimations.RESET);

		}
	}

	private void OnHurt(Area2D hittingArea) // Assuming HurtComponent passes the area
	{
		// Check if already dead to prevent taking damage/playing animations multiple times
		if (IsDead)
		{
			AnimationPlayer?.Play(LetterBlockAnimations.OnHurtDeadLetterBlock); // Play a specific "clink" animation maybe
			return;
		}

		TakeDamage();
	}

	private void TakeDamage()
	{
		HealthComponent?.TakeDamage(DamageTakenPerHit); // Use null check

		// Play hurt animation only if not dead AFTER taking damage
		if (HealthComponent != null && !HealthComponent.IsDead)
		{
			AnimationPlayer?.Play(LetterBlockAnimations.OnHurtLetterBlock);
		}
	}

	/// <summary>
	/// Called when health reaches zero. Starts the death sequence.
	/// </summary>
	private void HandleHealthDepleted()
	{
		if (IsDead)
		{
			return; // Already dead
		}

		IsDead = true;
		DeathSpriteEffect.Visible = true; // Show visual effect

		EmitSignal(SignalName.OnLetterDestructedSignal, IsTarget); // Notify parent immediately

		if (IsTarget)
		{
			DisableCollisions(); // Prevent further interaction if it's the target
			AnimationPlayer?.Play(LetterBlockAnimations.OnLetterBlockDyingTarget);
		}
		else
		{
			// Non-targets might just play a shorter dying animation or go straight to explode
			// depending on game design. Assuming a non-target dying animation here.
			AnimationPlayer?.Play(LetterBlockAnimations.OnLetterBlockDyingNotTarget);
			// Often, non-targets might just explode immediately when hit once if HP <= 0:
			// TriggerExplosion(); // Alternative: Explode immediately if not target
		}
	}

	private bool ValidateExports()
	{
		bool isValid = true;
		void CheckNode(GodotObject node, string name) // Use GodotObject for components too
		{
			if (node == null) { GD.PrintErr($"{Name}: Exported node/component '{name}' is null!"); isValid = false; }
		}

		CheckNode(Sprite, nameof(Sprite));
		CheckNode(Label, nameof(Label));
		CheckNode(CollisionShape, nameof(CollisionShape)); // Needed for StaticBody2D
		CheckNode(AnimationPlayer, nameof(AnimationPlayer));
		CheckNode(HitBox, nameof(HitBox));
		CheckNode(HurtComponent, nameof(HurtComponent));
		CheckNode(DeathSpriteEffect, nameof(DeathSpriteEffect));
		CheckNode(ExplosionsSprite2D, nameof(ExplosionsSprite2D));
		CheckNode(HealthComponent, nameof(HealthComponent));

		return isValid;
	}
	#endregion
}