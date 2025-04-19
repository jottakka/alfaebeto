using Alfaebeto.Components;
using Alfaebeto.CustomNodes;
using AlfaEBetto.Components;
using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Enemies;

public sealed partial class MeteorEnemyBase : StaticBody2D
{
	// --- Exports ---
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
	[Export] public HealthComponent HealthComponent { get; set; }
	[Export] public HurtComponent HurtComponent { get; set; }
	[Export] public Sprite2D Sprite2D { get; set; } // Assumed to have multiple frames for damage states
	[Export] public RandomItemDropComponent RandomItemDropComponent { get; set; }
	[Export] public HitBox HitBox { get; set; }
	[Export] public EnemyHurtBox EnemyHurtBox { get; set; } // Changed from HurtBox for clarity if distinct
	[Export] public AudioStreamPlayer2D HurtSound { get; set; }

	[ExportGroup("Appearance & Movement")]
	[Export(PropertyHint.Range, "0.5, 3.0, 0.1")] // Example range for scale
	public float MaxSizeProportion { get; set; } = 1.5f;
	[Export(PropertyHint.Range, "0.5, 3.0, 0.1")]
	public float MinSizeProportion { get; set; } = 1.0f;
	[Export] public float MaxSpeed { get; set; } = 60.0f;
	[Export] public float MinSpeed { get; set; } = 40.0f;

	[ExportGroup("Gameplay")]
	[Export(PropertyHint.Range, "1, 10, 1")] // How many health levels map to sprite frames
	private int _healthLevelIntervals = 6; // Default value, adjust based on sprite frames
	[Export] public int DamageFromPlayerSpecial { get; set; } = 10; // Damage taken from specific hit type

	// --- Private Fields ---
	private bool _isDead = false;
	private Vector2 _velocity;

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree(); // Cannot function without components
			return;
		}

		// --- Connect Signals ---
		// Use += for strongly-typed connections (requires delegates defined in components)
		HurtComponent.OnHurtSignal += OnHurt;
		GD.Print($"Connecting ScreenExited. Notifier valid? {IsInstanceValid(VisibleOnScreenNotifier)}");

		VisibleOnScreenNotifier.ScreenExited += HandleScreenExited; // Connect screen exited
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
		// Health signals connected in SetUpHealthComponent after validation
		SetUpHealthComponent();
		// ---------------------

		// --- Initialize State ---
		// Randomize scale and speed within defined ranges
		float scale = (float)GD.RandRange(MinSizeProportion, MaxSizeProportion);
		float speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
		_velocity = Vector2.Down * speed; // Move downwards
		Scale = Vector2.One * scale;

		// Set initial collision state (assuming extension methods exist)
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemy);
		this.ActivateCollisionMask(CollisionLayers.Player); // Example: Only detect player layer initially? Adjust as needed.

		// Start initial animation
		SetUpSpinAnimation();
	}

	public override void _ExitTree()
	{
		// Disconnect signals when removed from the tree
		if (IsInstanceValid(HurtComponent))
		{
			HurtComponent.OnHurtSignal -= OnHurt;
		}

		if (IsInstanceValid(HealthComponent))
		{
			HealthComponent.OnHealthDepletedSignal -= OnHealthDepleted;
			HealthComponent.OnHealthLevelChangeSignal -= OnHealthLevelChanged;
		}

		if (IsInstanceValid(VisibleOnScreenNotifier))
		{
			VisibleOnScreenNotifier.ScreenExited -= HandleScreenExited;
		}

		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Note: Moving a StaticBody2D by changing Position works, but it doesn't interact
		// with physics space like CharacterBody2D or RigidBody2D.
		// If collision response beyond simple detection is needed, consider changing the base type.
		if (!_isDead)
		{
			// Simple linear movement
			Position += _velocity * (float)delta;
		}
	}

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		if (VisibleOnScreenNotifier == null) { GD.PrintErr($"{Name}: Missing VisibleOnScreenNotifier!"); isValid = false; }

		if (HealthComponent == null) { GD.PrintErr($"{Name}: Missing HealthComponent!"); isValid = false; }

		if (HurtComponent == null) { GD.PrintErr($"{Name}: Missing HurtComponent!"); isValid = false; }

		if (Sprite2D == null) { GD.PrintErr($"{Name}: Missing Sprite2D!"); isValid = false; }

		if (RandomItemDropComponent == null) { GD.PrintErr($"{Name}: Missing RandomItemDropComponent!"); isValid = false; }

		if (HitBox == null) { GD.PrintErr($"{Name}: Missing HitBox!"); isValid = false; }

		if (EnemyHurtBox == null) { GD.PrintErr($"{Name}: Missing EnemyHurtBox!"); isValid = false; }

		if (HurtSound == null) { GD.PrintErr($"{Name}: Missing HurtSound!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Sets up the initial spinning animation, choosing a random direction.
	/// </summary>
	private void SetUpSpinAnimation()
	{
		// Check validity before playing
		if (!IsInstanceValid(AnimationPlayer))
		{
			return;
		}

		bool spinClockwise = GD.Randi() % 2 == 0; // 50% chance for either direction

		if (spinClockwise)
		{
			AnimationPlayer.Play(EnemyAnimations.MeteorEnemySpin);
		}
		else
		{
			// Play backwards requires speed scale -1, Play(name, -1, speed, true)
			// Or have a separate "SpinReverse" animation if easier
			AnimationPlayer.Play(EnemyAnimations.MeteorEnemySpin, customSpeed: -1.0f); // Play in reverse
		}
	}

	/// <summary>
	/// Handles the hurt signal from the HurtComponent.
	/// </summary>
	private void OnHurt(Area2D enemyArea) // Assuming enemyArea is the hitbox that hit us
	{
		// Check validity of this node and components
		if (!IsInstanceValid(this) || _isDead || !IsInstanceValid(HealthComponent))
		{
			return;
		}

		// Determine damage based on the type of area that hit
		// Using 'is' pattern matching
		int damageToTake = 0;
		switch (enemyArea)
		{
			case PlayerSpecialHurtBox _: // Example check - use actual type or group
				damageToTake = DamageFromPlayerSpecial; // Use exported damage value
				break;
			// Add other cases if needed
			// case PlayerRegularHurtBox _:
			//     damageToTake = DamageFromPlayerRegular;
			//     break;
			default:
				// Optionally log unknown hit sources
				// GD.Print($"{Name} hurt by unhandled area: {enemyArea?.Name}");
				break;
		}

		// Apply damage and play sound if damage was determined
		if (damageToTake > 0)
		{
			HealthComponent.TakeDamage(damageToTake);
			HurtSound?.Play(); // Play hurt sound
							   // Optionally play a visual hurt effect animation via AnimationPlayer or EffectsPlayer
							   // AnimationPlayer?.Play("HurtFlash"); // Example
		}
	}

	/// <summary>
	/// Updates the sprite frame based on the health level signal.
	/// Assumes sprite frames correspond to health levels (0 = full health, higher = more damaged).
	/// </summary>
	/// <param name="healthLevel">The current health level (0 near full, higher near empty).</param>
	private void OnHealthLevelChanged(int healthLevel)
	{
		// Check validity before accessing sprite
		if (!IsInstanceValid(Sprite2D))
		{
			return;
		}

		// Directly use the healthLevel as the frame index
		// Ensure healthLevel is clamped appropriately within HealthComponent if necessary
		// Or clamp here based on actual available frames:
		// int maxFrame = Sprite2D.Hframes * Sprite2D.Vframes - 1; // If using sprite sheet frames
		// Sprite2D.Frame = Mathf.Clamp(healthLevel, 0, maxFrame);
		Sprite2D.Frame = healthLevel; // Assuming healthLevel corresponds directly to frame index
	}

	/// <summary>
	/// Handles the health depletion signal. Plays death sequence.
	/// </summary>
	private void OnHealthDepleted()
	{
		if (_isDead)
		{
			return; // Prevent multiple triggers
		}

		_isDead = true;

		// Stop movement by disabling physics process
		SetPhysicsProcess(false);
		// Optionally clear velocity if needed elsewhere: Velocity = Vector2.Zero;

		// Deactivate collisions using helper components/methods
		// Check validity before calling methods
		this.ResetCollisionLayerAndMask(); // Reset StaticBody2D collision
		HitBox?.DeactivateCollisions();
		EnemyHurtBox?.DeactivateCollisions();

		// Trigger item drop (check validity)
		RandomItemDropComponent?.DropRandomItem(GlobalPosition);

		// Play death animation (check validity)
		AnimationPlayer?.Play(EnemyAnimations.MeteorEnemyDeath);
	}

	/// <summary>
	/// Handles the animation finished signal, specifically for the death animation.
	/// </summary>
	private void OnAnimationFinished(StringName animationName)
	{
		// Check validity of this node first
		if (!IsInstanceValid(this))
		{
			return;
		}

		// QueueFree only after the death animation completes
		if (animationName == EnemyAnimations.MeteorEnemyDeath)
		{
			QueueFree();
		}
		// Handle other animation finishes if needed
	}

	/// <summary>
	/// Configures the HealthComponent and connects its signals.
	/// </summary>
	private void SetUpHealthComponent()
	{
		// Null check performed in _Ready via ValidateExports
		// Ensure property names and typos match your HealthComponent script exactly
		HealthComponent.EmitInBetweenSignals = true; // Corrected typo
		HealthComponent.HealthLevelSignalsIntervals = _healthLevelIntervals; // Corrected typo & use export

		// Connect signals using strongly-typed +=
		// Remove first to prevent duplicates if _Ready is somehow called twice
		HealthComponent.OnHealthDepletedSignal -= OnHealthDepleted;
		HealthComponent.OnHealthLevelChangeSignal -= OnHealthLevelChanged;
		// Connect
		HealthComponent.OnHealthDepletedSignal += OnHealthDepleted;
		HealthComponent.OnHealthLevelChangeSignal += OnHealthLevelChanged;
	}

	/// <summary>
	/// Handles the screen exited signal from the notifier.
	/// </summary>
	private void HandleScreenExited() =>
		// Use CallDeferred for safety from signal handler
		CallDeferred(MethodName.QueueFree);
}
