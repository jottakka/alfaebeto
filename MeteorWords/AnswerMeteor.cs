using System.Xml.Linq;
using Godot;

public sealed partial class AnswerMeteor : StaticBody2D
{
	// --- Exports ---
	[Export] public HitBox HitBox { get; set; }
	[Export] public HurtComponent HurtComponent { get; set; }
	[Export] public EnemyHurtBox HurtBox { get; set; } // Note: You have HurtComponent and EnemyHurtBox? Ensure correct usage.
	[Export] public Sprite2D CrackSprite2D { get; set; }
	[Export] public HealthComponent HealthComponent { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public AnimationPlayer EffectsPlayer { get; set; } // For effects like 'hurt'
	[Export] public Label OptionText { get; set; }

	[ExportGroup("Configuration")]
	[Export(PropertyHint.Range, "1,10,1")] // Export health levels, allow 1-10 levels in editor
	private int _healthLevels = 4;

	// --- Signals ---
	[Signal]
	public delegate void OnDestroiedSignalEventHandler(bool isTarget);

	// --- Public Properties ---
	public bool IsTarget { get; set; } = false;

	// --- Private Fields ---
	private bool _isDestroyed = false;

	// --- Constants ---
	private static class MeteorAnimations
	{
		// Define animation names used in this script
		public const string AnswerMeteorMoving = "AnswerMeteorMoving";
		public const string AnswerMeteorFade = "AnswerMeteorFade";
		public const string AnswerMeteorTargetDeath = "AnswerMeteorTargetDeath";
		public const string AnswerMeteorNotTargetDeath = "AnswerMeteorNotTargetDeath";
		public const string AnswerMeteorHurt = "AnswerMeteorHurt";
	}

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			// Consider SetProcess(false), SetPhysicsProcess(false) or QueueFree()
			QueueFree();
			return;
		}

		// Assuming these are extension methods or defined in a base class
		this.ResetCollisionLayerAndMask();
		ActivateBodyCollisions();

		SetUpHealthComponent(); // Setup depends on HealthComponent being valid

		// Connect signals using safer methods
		// Assuming OnHurtSignal is defined in HurtComponent like: [Signal] public delegate void OnHurtSignalEventHandler(Area2D area);
		HurtComponent.Connect(nameof(HurtComponent.OnHurtSignal), Callable.From<Area2D>(OnHurt));

		AnimationPlayer?.Play(MeteorAnimations.AnswerMeteorMoving);
	}

	// --- Public Methods ---

	/// <summary>
	/// Initiates the fade-out sequence when the parent meteor is destroyed.
	/// </summary>
	public void DestroyCommand()
	{
		// Check _isDestroyed flag first for efficiency
		if (_isDestroyed) return;

		// Set flag early to prevent race conditions
		_isDestroyed = true;

		DeactivateCollisions();
		// Use null-conditional operator for safety
		AnimationPlayer?.Play(MeteorAnimations.AnswerMeteorFade);
		// Note: This doesn't emit OnDestroiedSignal. That's handled by OnHealthDepleted.
	}

	/// <summary>
	/// Deactivates all collision shapes associated with this meteor.
	/// </summary>
	public void DeactivateCollisions()
	{
		// Use null-conditional operators for safety
		HitBox?.DeactivateCollisions();
		HurtBox?.DeactivateCollisions();
		DeactivateBodyCollisions();
	}

	/// <summary>
	/// Activates all collision shapes associated with this meteor.
	/// </summary>
	public void ActivateCollisions()
	{
		// Use null-conditional operators for safety
		HitBox?.ActivateCollisionsMasks();
		HurtBox?.ActivateCollisionsMasks();
		ActivateBodyCollisions();
	}

	// --- Private Methods ---

	private bool ValidateExports()
	{
		bool isValid = true;
		if (HitBox == null) { GD.PrintErr($"{Name}: Missing HitBox!"); isValid = false; }
		if (HurtComponent == null) { GD.PrintErr($"{Name}: Missing HurtComponent!"); isValid = false; }
		if (HurtBox == null) { GD.PrintErr($"{Name}: Missing HurtBox!"); isValid = false; } // Check if both HurtBox and HurtComponent are needed?
		if (CrackSprite2D == null) { GD.PrintErr($"{Name}: Missing CrackSprite2D!"); isValid = false; }
		if (HealthComponent == null) { GD.PrintErr($"{Name}: Missing HealthComponent!"); isValid = false; }
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }
		if (EffectsPlayer == null) { GD.PrintErr($"{Name}: Missing EffectsPlayer!"); isValid = false; }
		if (OptionText == null) { GD.PrintErr($"{Name}: Missing OptionText!"); isValid = false; }
		return isValid;
	}

	private void DeactivateBodyCollisions()
	{
		// Assuming this is an extension method or defined in a base class
		this.ResetCollisionLayerAndMask();
	}

	private void ActivateBodyCollisions()
	{
		// Assuming this is an extension method or defined in a base class
		// Also assuming CollisionLayers enum exists
		this.ActivateCollisionLayer(CollisionLayers.MeteorEnemy);
	}

	/// <summary>
	/// Called by HealthComponent when health reaches zero.
	/// </summary>
	private void OnHealthDepleted()
	{
		if (_isDestroyed) return; // Prevent multiple triggers

		_isDestroyed = true;
		DeactivateBodyCollisions(); // Deactivate static body collision

		// Play death animation based on whether it was the target
		string deathAnimation = IsTarget
			? MeteorAnimations.AnswerMeteorTargetDeath
			: MeteorAnimations.AnswerMeteorNotTargetDeath;

		AnimationPlayer?.Play(deathAnimation);

		// Emit the signal indicating destruction and whether it was the target
		// Using nameof() with the delegate name is safe.
		EmitSignal(nameof(OnDestroiedSignalEventHandler), IsTarget);
		// Or if using Godot 4 StringNames: EmitSignal(SignalName.OnDestroiedSignal, IsTarget);
	}

	/// <summary>
	/// Called by HurtComponent when the meteor's hurtbox is hit.
	/// </summary>
	/// <param name="enemyArea">The area that caused the hurt (e.g., player bullet hitbox).</param>
	private void OnHurt(Area2D enemyArea)
	{
		if (_isDestroyed) return; // Don't take damage if already destroyed

		// Apply damage via HealthComponent (ensure HealthComponent is not null)
		// Consider making damage amount an export or parameter?
		HealthComponent?.TakeDamage(10);

		// Play hurt visual effect if not dead yet
		// Check IsDead *after* taking damage
		if (HealthComponent != null && !HealthComponent.IsDead)
		{
			EffectsPlayer?.Play(MeteorAnimations.AnswerMeteorHurt);
		}
	}

	/// <summary>
	/// Called by HealthComponent when health crosses a defined level threshold.
	/// Updates the visibility of the crack sprite.
	/// </summary>
	/// <param name="healthLevel">The current health level (e.g., 3, 2, 1).</param>
	private void OnHealthLevelChanged(int healthLevel)
	{
		if (CrackSprite2D == null) return; // Null check

		// Prevent division by zero and handle potential negative levels gracefully
		if (_healthLevels <= 0) return;

		// Calculate alpha based on remaining health levels (more damage = more visible crack)
		// Alpha goes from nearly 0 (full health) up towards 1 (low health)
		// Example: 4 levels -> healthLevel 3 -> alpha ~0.25; healthLevel 1 -> alpha ~0.75
		float alpha = 1.0f - ((float)healthLevel / _healthLevels);

		// Clamp alpha between 0 and 1 just in case
		alpha = Mathf.Clamp(alpha, 0.0f, 1.0f);

		// Apply the new alpha to the crack sprite's modulate property
		Color newColor = CrackSprite2D.Modulate;
		newColor.A = alpha;
		CrackSprite2D.Modulate = newColor;
	}

	/// <summary>
	/// Configures the HealthComponent and connects its signals.
	/// </summary>
	private void SetUpHealthComponent()
	{
		// Null check performed in _Ready before calling this
		HealthComponent.EmmitInBetweenSignals = true; // Ensure typo 'Emmit' is correct in HealthComponent script
		HealthComponent.HeathLevelSignalsIntervals = _healthLevels; // Ensure typo 'Heath' is correct in HealthComponent script

		// Connect signals using safer methods
		// Assuming signals are defined like: [Signal] public delegate void OnHealthDepletedSignalEventHandler();
		HealthComponent.Connect(nameof(HealthComponent.OnHealthDepletedSignal), Callable.From(OnHealthDepleted));
		// Assuming signal is defined like: [Signal] public delegate void OnHealthLevelChangeSignalEventHandler(int level);
		HealthComponent.Connect(nameof(HealthComponent.OnHealthLevelChangeSignal), Callable.From<int>(OnHealthLevelChanged));
	}
}
