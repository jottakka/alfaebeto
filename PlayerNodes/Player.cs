using Alfaebeto.Components;
using AlfaEBetto.Components;
using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.PlayerNodes;

public sealed partial class Player : CharacterBody2D
{
	// --- Exports ---
	[Export] public Sprite2D Sprite2D { get; set; }
	[Export] public CollisionPolygon2D CollisionPolygon2D { get; set; } // Or CollisionShape2D
	[Export] public PlayerInputProcessor PlayerInputProcessor { get; set; }
	[Export] public Marker2D MuzzlePosition { get; set; }
	[Export] public WeaponComponent WeaponComponent { get; set; }
	[Export] public HitBox HitBox { get; set; } // For dealing damage?
	[Export] public HurtComponent HurtComponent { get; set; } // For receiving damage
	[Export] public HealthComponent HealthComponent { get; set; }
	[Export] public PlayerShield PlayerShield { get; set; } // Assumed to be a child node or handle its own positioning
	[Export] public AnimationPlayer EffectsPlayer { get; set; }
	[Export] public PlayerItemCollectingComponent PlayerItemCollectingComponent { get; set; } // Handles collecting items

	[ExportGroup("Movement & Stats")]
	[Export] public float Speed { get; set; } = 600.0f;
	[Export] public int DamageOnHurt { get; set; } = 10; // Damage taken when hurt (if shield inactive)
														 // Removed collision pushback factor as MoveAndSlide handles collisions better

	// --- Signals ---
	[Signal] public delegate void OnMoneyChangedSignalEventHandler(long money);
	[Signal] public delegate void OnGemAddedSignalEventHandler(GemType gemType); // Pass the enum directly
	[Signal] public delegate void OnPlayerDeathSignalEventHandler();

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Player may not function correctly.");
			SetPhysicsProcess(false); // Disable physics if setup fails
			return;
		}

		MotionMode = MotionModeEnum.Floating; // Good for top-down or space shooters

		// Assuming SetVisibilityZOrdering extension method exists
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);
		// Setup initial collision layers/masks
		SetupCollisions();

		// --- Connect Signals ---
		// Use += for strongly-typed connections
		if (IsInstanceValid(HurtComponent))
		{
			HurtComponent.OnHurtSignal += OnHurt;
		}

		if (IsInstanceValid(HealthComponent))
		{
			HealthComponent.OnHealthDepletedSignal += OnDeath;
		}
		// ---------------------
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
			HealthComponent.OnHealthDepletedSignal -= OnDeath;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Check dependencies validity
		if (!IsInstanceValid(PlayerInputProcessor) || !IsInstanceValid(HealthComponent))
		{
			// Cannot move without input or health state
			Velocity = Vector2.Zero; // Stop moving if components are invalid
			return;
		}

		// Don't process movement if dead
		if (HealthComponent.IsDead)
		{
			Velocity = Vector2.Zero;
			// Optionally disable physics process: SetPhysicsProcess(false);
			return;
		}

		// Get movement input
		Vector2 movementDirection = PlayerInputProcessor.MovementDirection;
		// Calculate velocity (units per second)
		Vector2 targetVelocity = movementDirection * Speed;

		// Apply velocity using CharacterBody2D's method
		Velocity = targetVelocity; // Assign to Velocity property
		MoveAndSlide(); // Godot handles collisions and delta time internally

		// --- Screen Clamping (After Movement) ---
		// Keep player within screen bounds
		Rect2 viewportRect = GetViewportRect();
		GlobalPosition = GlobalPosition.Clamp(Vector2.Zero, viewportRect.Size);
		// -----------------------------------------

		// --- Shield Positioning (Simplified) ---
		// Assuming PlayerShield is a child node, its position relative to the player is fixed.
		// If it needs separate logic or activation visuals, handle it within PlayerShield script.
		// If PlayerShield needs to be exactly at Player position:
		// if (IsInstanceValid(PlayerShield)) { PlayerShield.GlobalPosition = GlobalPosition; }
		// -----------------------------------------
	}

	// --- Public Methods ---

	/// <summary>
	/// Emits a signal indicating money amount has changed.
	/// </summary>
	/// <param name="money">The amount of money added (can be negative).</param>
	public void AddMoney(long money) =>
		// Use SignalName for type safety (Godot 4+)
		EmitSignal(SignalName.OnMoneyChangedSignal, money);

	/// <summary>
	/// Emits a signal indicating a gem was collected.
	/// </summary>
	/// <param name="gemType">The type of gem collected.</param>
	public void AddGem(GemType gemType) => EmitSignal(SignalName.OnGemAddedSignal, (Variant)(int)gemType); // Cast enum to int/Variant for signal emission

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes/components are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (Sprite2D == null) { GD.PrintErr($"{Name}: Missing Sprite2D!"); isValid = false; }

		if (CollisionPolygon2D == null) { GD.PrintErr($"{Name}: Missing CollisionPolygon2D!"); isValid = false; }

		if (PlayerInputProcessor == null) { GD.PrintErr($"{Name}: Missing PlayerInputProcessor!"); isValid = false; }

		if (MuzzlePosition == null) { GD.PrintErr($"{Name}: Missing MuzzlePosition (Marker2D)!"); isValid = false; }

		if (WeaponComponent == null) { GD.PrintErr($"{Name}: Missing WeaponComponent!"); isValid = false; }

		if (HitBox == null) { GD.PrintErr($"{Name}: Missing HitBox!"); isValid = false; }

		if (HurtComponent == null) { GD.PrintErr($"{Name}: Missing HurtComponent!"); isValid = false; }

		if (HealthComponent == null) { GD.PrintErr($"{Name}: Missing HealthComponent!"); isValid = false; }

		if (PlayerShield == null) { GD.PrintErr($"{Name}: Missing PlayerShield!"); isValid = false; }

		if (EffectsPlayer == null) { GD.PrintErr($"{Name}: Missing EffectsPlayer!"); isValid = false; }

		if (PlayerItemCollectingComponent == null) { GD.PrintErr($"{Name}: Missing PlayerItemCollectingComponent!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Sets up the initial collision layer and mask for the player body.
	/// </summary>
	private void SetupCollisions() // Renamed from SettingCollisions
	{
		// Assuming ResetCollisionLayerAndMask and ActivateCollisionLayer/Mask are extension methods
		this.ResetCollisionLayerAndMask();
		// Player IS on the Player layer
		this.ActivateCollisionLayer(CollisionLayers.Player);

		// Player MASK checks for things it can collide with or detect
		this.ActivateCollisionMask(CollisionLayers.Collectables); // For item collection
		this.ActivateCollisionMask(CollisionLayers.MeteorEnemy);   // For collision damage
		this.ActivateCollisionMask(CollisionLayers.WordEnemy);     // For collision damage
		this.ActivateCollisionMask(CollisionLayers.EnemyAmmo);     // For collision damage
																   // Add other layers the player body needs to interact with (e.g., Walls)
	}

	/// <summary>
	/// Handles the player's death sequence.
	/// </summary>
	private void OnDeath()
	{
		GD.Print($"{Name}: Player Died!");
		// Disable further physics processing
		SetPhysicsProcess(false);
		Velocity = Vector2.Zero; // Stop movement immediately

		// Optionally hide player, disable collisions, play death animation
		Hide(); // Example
		CollisionLayer = 0; // Disable all collision interactions
		CollisionMask = 0;
		// EffectsPlayer?.Play("DeathAnimation"); // Example

		// Emit death signal for game manager
		EmitSignal(SignalName.OnPlayerDeathSignal);
		// Consider deferring QueueFree if animations need to play
		// QueueFree();
	}

	/// <summary>
	/// Handles the hurt signal from the HurtComponent.
	/// </summary>
	private void OnHurt(Area2D enemyArea) // enemyArea is the hitbox/area that hurt the player
	{
		// Check validity and if shield is active
		if (!IsInstanceValid(this) || !IsInstanceValid(HealthComponent) || !IsInstanceValid(PlayerShield))
		{
			return;
		}

		// If shield is active, player doesn't take damage (shield handles its own logic)
		if (PlayerShield.IsActive)
		{
			// Maybe play a shield hit effect?
			// EffectsPlayer?.Play("ShieldHit");
			return;
		}

		// Shield is not active, take damage
		HealthComponent.TakeDamage(DamageOnHurt); // Use exported damage value

		// Play hurt animation/effect (check validity)
		EffectsPlayer?.Play(PlayerAnimations.OnPlayerHurtBlink);

		// Optional: Apply brief invincibility or knockback here if desired
	}
}
