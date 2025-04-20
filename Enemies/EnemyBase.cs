using System;
using Alfaebeto;
using Alfaebeto.Components; // Corrected namespace
using Alfaebeto.CustomNodes; // Corrected namespace
using AlfaEBetto.Components;
using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using AlfaEBetto.Weapons;
using Godot;

namespace AlfaEBetto.Enemies; // Corrected namespace

/// <summary>
/// Base class for standard enemies that move, take damage, and drop items.
/// Uses CharacterBody2D for movement.
/// </summary>
public sealed partial class EnemyBase : CharacterBody2D // Correct base type
{
	#region Exports
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public HitBox HitBox { get; set; } // For detecting player attacks
	[Export] public EnemyHurtBox HurtBox { get; set; } // For player detection (sets its own layer)
	[Export] public HurtComponent HurtComponent { get; set; }
	[Export] public HealthComponent HealthComponent { get; set; }
	[Export] public CoinSpawnerComponent CoinSpawnerComponent { get; set; }
	[Export] public Sprite2D SplatsSprite2D { get; set; } // For death splat effect

	[ExportGroup("Movement & Stats")]
	[Export] public float Speed { get; set; } = 60.0f;
	[Export] public float KnockBackFactor { get; set; } = 50.0f; // Consider using impulses/velocity changes for knockback
	[Export(PropertyHint.Range, "1, 10, 1")]
	private int _splatFrameCount = 5; // Default value
	#endregion

	#region Public Properties (Set by Spawner/Creator)
	/// <summary>Intended initial global position, set by the spawner/builder.</summary>
	public Vector2 InitialPosition { get; set; } = Vector2.Zero;
	/// <summary>Intended initial velocity, set by the spawner/builder.</summary>
	public Vector2 SpawnInitialVelocity { get; set; } = Vector2.Zero;
	#endregion

	#region Private Fields
	private Player _cachedPlayer;
	private bool _isSpawning = false; // Track if in spawning animation
	private bool _isInitialized = false;
	private bool _isDead = false; // Added explicit death flag
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		Initialize();
		GD.Print($"{Name} _Ready: Initial GlobalPosition = {GlobalPosition}, Initial Position = {Position}"); // Keep log for verification
	}

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes. Deactivating.");
			QueueFree();
			return;
		}

		// --- *** POSITIONING LOGIC *** ---
		// Set initial position using the value provided BEFORE _Ready by the builder/spawner.
		GlobalPosition = InitialPosition;
		// --- *** END POSITIONING LOGIC *** ---

		CachePlayerReference();
		SetupVisuals();
		ConnectSignals();
		SetupInitialState();

		_isInitialized = true;
	}

	public override void _ExitTree() => DisconnectSignals();

	public override void _PhysicsProcess(double delta)
	{
		// Don't process if not initialized, dead, or hurt
		if (!_isInitialized || _isDead || (HurtComponent?.IsHurt ?? false))
		{
			// Optional: Apply friction or stop movement if hurt/dead
			// Velocity = Velocity.Lerp(Vector2.Zero, 0.1f); MoveAndSlide();
			return;
		}

		Vector2 currentVelocity = Vector2.Zero;
		if (_isSpawning)
		{
			// Apply initial spawn velocity (e.g., being pushed out of spawner)
			currentVelocity = SpawnInitialVelocity;
			// Note: Consider adding drag/friction or reducing this velocity over time.
			// For now, assumes spawn animation handles transition.
		}
		else // Not spawning, normal behavior (e.g., move towards player)
		{
			if (IsInstanceValid(_cachedPlayer))
			{
				Vector2 direction = GlobalPosition.DirectionTo(_cachedPlayer.GlobalPosition);
				currentVelocity = direction * Speed;
			}
			else
			{
				currentVelocity = Vector2.Zero; // Stop if no player
			}
		}

		Velocity = currentVelocity; // Set CharacterBody2D velocity
		MoveAndSlide();
	}
	#endregion

	#region Initialization Helpers
	private void CachePlayerReference()
	{
		if (IsInstanceValid(Global.Instance))
		{
			_cachedPlayer = Global.Instance.Player;
			if (!IsInstanceValid(_cachedPlayer))
			{
				GD.PrintErr($"{Name} ({GetPath()}): Global.Instance.Player is invalid in _Ready.");
			}
		}
		else
		{
			GD.PrintErr($"{Name} ({GetPath()}): Global.Instance is invalid in _Ready.");
		}
	}

	private void SetupVisuals()
	{
		// Randomize the splat sprite frame
		if (IsInstanceValid(SplatsSprite2D) && _splatFrameCount > 0)
		{
			SplatsSprite2D.Visible = false; // Start hidden
			SplatsSprite2D.Frame = GD.RandRange(0, _splatFrameCount - 1); // Correct range
		}
		// Set Z index
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies); // Check enum name
	}

	private void ConnectSignals()
	{
		if (IsInstanceValid(HurtComponent))
		{
			HurtComponent.OnHurtSignal += OnHurt;
		}

		if (IsInstanceValid(HealthComponent))
		{
			HealthComponent.OnHealthDepletedSignal += OnDeath;
		}

		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished += OnAnimationFinished;
		}
	}

	private void DisconnectSignals()
	{
		if (IsInstanceValid(HurtComponent))
		{
			HurtComponent.OnHurtSignal -= OnHurt;
		}

		if (IsInstanceValid(HealthComponent))
		{
			HealthComponent.OnHealthDepletedSignal -= OnDeath;
		}

		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}
	}

	private void SetupInitialState()
	{
		// Deactivate collisions initially IF spawning, otherwise activate
		if (_isSpawning)
		{
			DeactivateCollisions();
			AnimationPlayer?.Play(EnemyAnimations.EnemySpawn); // Start spawn anim immediately
		}
		else
		{
			// If not spawning, become active immediately
			OnReadyToAction();
		}
	}

	private bool ValidateExports()
	{
		// ... (validation code remains the same) ...
		bool isValid = true;
		void CheckNode(GodotObject node, string name) { if (node == null) { GD.PrintErr($"{Name} ({GetPath()}): Missing export '{name}'!"); isValid = false; } }

		CheckNode(AnimationPlayer, nameof(AnimationPlayer));
		CheckNode(HitBox, nameof(HitBox));
		CheckNode(HurtBox, nameof(HurtBox));
		CheckNode(HurtComponent, nameof(HurtComponent));
		CheckNode(HealthComponent, nameof(HealthComponent));
		CheckNode(CoinSpawnerComponent, nameof(CoinSpawnerComponent));
		CheckNode(SplatsSprite2D, nameof(SplatsSprite2D));
		return isValid;
	}
	#endregion

	#region Public API
	/// <summary>
	/// Forces the enemy to start its death sequence immediately.
	/// Useful for external triggers like spawner cleanup.
	/// </summary>
	public void ForceDespawn() // New method
	{
		// Call the same method that handling health depletion calls
		// This ensures animations play, coins drop, etc.
		OnDeath();
	}
	/// <summary>
	/// Sets the enemy state to spawning. Called externally by the spawner.
	/// Ensures spawn animation plays and collisions are initially off.
	/// </summary>
	public void SetAsSpawning(bool spawning = true)
	{
		_isSpawning = spawning;
		Visible = true; // Ensure visible when spawning starts

		// If _Ready hasn't run yet, the flag will be checked there.
		// If _Ready has run, play the animation now (if not already playing).
		if (_isInitialized && spawning && AnimationPlayer?.CurrentAnimation != EnemyAnimations.EnemySpawn)
		{
			DeactivateCollisions(); // Ensure collisions off during spawn anim
			AnimationPlayer?.Play(EnemyAnimations.EnemySpawn);
		}
		else if (!spawning)
		{
			// Force transition to active state if called with false externally
			OnReadyToAction();
		}
	}
	#endregion

	#region Internal Logic & Handlers

	/// <summary>Called when the enemy is ready for normal actions (after spawning or reset).</summary>
	private void OnReadyToAction()
	{
		_isSpawning = false; // Ensure spawning state is false
		if (!_isDead) // Only activate if not dead
		{
			AnimationPlayer?.Play(EnemyAnimations.EnemyBugMoving); // Or Idle?
			ActivateCollisions();
		}
	}

	private void OnHurt(Area2D hittingArea)
	{
		if (_isDead || _isSpawning)
		{
			return; // Added check for HurtComponent cooldown
		}

		// Apply knockback first? Or damage? Depends on feel. Let's do damage first.
		int damageToTake = DetermineDamage(hittingArea);

		if (damageToTake > 0)
		{
			HealthComponent?.TakeDamage(damageToTake); // HealthComponent handles validity
			if (HealthComponent != null && !HealthComponent.IsDead)
			{
				AnimationPlayer?.Play(EnemyAnimations.EnemyBugHurtBlink); // Play hurt animation
				ApplyKnockback(hittingArea); // Apply knockback only if hurt but not dead
			}
			// Note: OnDeath handles logic if health depleted
		}
	}

	private int DetermineDamage(Area2D hittingArea)
	{
		// Add this print first!
		GD.Print($"DetermineDamage called. Hitting area: {hittingArea?.Name}, Layer: {hittingArea?.CollisionLayer}, Groups: {string.Join(",", hittingArea?.GetGroups() ?? [])}");

		int damage = 15;
		if (hittingArea is PlayerSpecialHurtBox)
		{
			damage = 20;
		}
		else if (hittingArea?.GetOwner() is OwlFriend owlFriend)
		{
			damage = owlFriend.HitPoints;
		}
		// *** THIS IS THE IMPORTANT CHECK FOR THE LASER ***
		else if (hittingArea != null && hittingArea.IsInGroup("PlayerProjectile"))
		{
			damage = 5; // Default projectile damage (Adjust as needed)
		}
		else
		{
			GD.Print($"{Name} hurt by unhandled area type/group."); // You might be seeing this log
		}
		GD.Print($"Damage determined: {damage}"); // Check if this is > 0
		return damage;
	}

	private void ApplyKnockback(Area2D hittingArea)
	{
		if (!IsInstanceValid(hittingArea))
		{
			return;
		}

		Vector2 knockDirection = hittingArea.GlobalPosition.DirectionTo(GlobalPosition).Normalized();
		// Applying knockback to CharacterBody2D: Modify Velocity
		Velocity += knockDirection * KnockBackFactor; // Add impulse
													  // MoveAndSlide() in _PhysicsProcess will handle the movement
	}

	private void OnDeath()
	{
		if (_isDead)
		{
			return;
		}

		_isDead = true;

		Velocity = Vector2.Zero; // Stop movement
		SetPhysicsProcess(false); // Stop physics updates
		DeactivateCollisions();

		// Make splat visible just before main death animation
		if (SplatsSprite2D != null)
		{
			SplatsSprite2D.Visible = true;
		}

		AnimationPlayer?.Play(EnemyAnimations.EnemyBugDie);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (!IsInstanceValid(this))
		{
			return;
		}

		if (animationName == EnemyAnimations.EnemyBugDie)
		{
			CoinSpawnerComponent?.SpawnCoins(GlobalPosition); // Spawn coins on death completion
			QueueFree();
		}
		else if (animationName == EnemyAnimations.EnemyBugHurtBlink)
		{
			OnHurtStateFinished(); // Ready to act again after hurt anim
		}
		else if (animationName == EnemyAnimations.EnemySpawn)
		{
			OnReadyToAction(); // Ready to act after spawn anim
		}
	}

	/// <summary>Called when the hurt animation finishes.</summary>
	private void OnHurtStateFinished()
	{
		if (_isDead)
		{
			return; // Don't reactivate if died during hurt
		}

		// HurtComponent should still be valid based on _Ready check
		HurtComponent?.OnHurtCooldownTimeout(); // Reset HurtComponent state
		if (!_isSpawning) // Don't activate collisions if still in (an interrupted?) spawn state
		{
			ActivateCollisions();
			AnimationPlayer?.Play(EnemyAnimations.EnemyBugMoving); // Return to moving/idle
		}
	}

	private void ActivateCollisions()
	{
		// Activate main body collision (adjust layer/mask as needed for specific enemy)
		this.ResetCollisionLayerAndMask(); // Clear first
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemy); // Example layer for body
		this.ActivateCollisionMask(CollisionLayers.Player); // Example mask
		this.ActivateCollisionMask(CollisionLayers.Enviroment); // Example mask

		// Activate component collision areas
		HurtBox?.SetCollisionLayerBasedOnParent(); // HurtBox sets its layer
		HitBox?.ActivateCollisionsMasks(); // HitBox sets its mask? Or maybe layer? Check HitBox script.
	}

	private void DeactivateCollisions()
	{
		this.ResetCollisionLayerAndMask(); // Reset CharacterBody2D layer/mask
		HurtBox?.DeactivateCollisions();
		HitBox?.DeactivateCollisions();
	}
	#endregion
}