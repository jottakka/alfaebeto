using AlfaEBetto.CustomNodes;
using AlfaEBetto.Enemies;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.Weapons
{
	public sealed partial class OwlFriend : CharacterBody2D
	{
		// --- Exports ---
		[Export] public Area2D DetectionArea { get; set; }
		[Export] public Timer CooldownTimer { get; set; }
		[Export] public Area2D HurtBox { get; set; } // HurtBox for the Owl itself?
		[Export] public AnimationPlayer AnimationPlayer { get; set; }

		[ExportGroup("Gameplay Stats")]
		[Export] public int HitPoints { get; set; } = 100; // Damage dealt on hit? Changed from field to property
		[Export] public float AttackSpeed { get; set; } = 400.0f;
		[Export] public float RegularSpeed { get; set; } = 300.0f;
		[Export(PropertyHint.Range, "0, 500, 5")] public float TargetRetreatDistance { get; set; } = 100.0f;
		[Export(PropertyHint.Range, "0, 500, 5")] public float PlayerFollowDistance { get; set; } = 100.0f;
		[Export(PropertyHint.Range, "0, 100, 1")] public float OscillationAmplitude { get; set; } = 20.0f;
		[Export(PropertyHint.Range, "100000.0, 1000000.0, 10000")] public float OscillationPeriodMs { get; set; } = 500000.0f;

		// --- Properties ---
		public EnemyBase Target { get; private set; } = null;
		public bool HasTarget => IsInstanceValid(Target); // Use IsInstanceValid for Godot nodes

		// --- Private Fields ---
		private Player _cachedPlayer = null; // Cache player reference - Initialized in OnMainNodeReady
		private bool _attackReady = true;

		public override void _Ready()
		{
			if (!ValidateExports())
			{
				GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
				SetPhysicsProcess(false); // Keep physics off until ready
				return;
			}

			// Don't cache player here, wait for Global signal

			// Assuming extension methods exist
			this.ResetCollisionLayerAndMask();
			DetectionArea?.ResetCollisionLayerAndMask(); // Check validity
			this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies); // Or specific ally layer

			// Configure Detection Area (check validity)
			if (IsInstanceValid(DetectionArea))
			{
				DetectionArea.ActivateCollisionMask(CollisionLayers.RegularEnemyHitBox);
				DetectionArea.AreaEntered += OnDetectionAreaEntered;
				DetectionArea.AreaExited += OnDetectionAreaExited;
			}

			// Configure HurtBox (check validity)
			if (IsInstanceValid(HurtBox))
			{
				HurtBox.AreaEntered += OnHurtBoxEntered;
			}

			// Configure Cooldown Timer (check validity)
			if (IsInstanceValid(CooldownTimer))
			{
				CooldownTimer.OneShot = true;
				CooldownTimer.Timeout += OnCooldownTimerTimeout;
			}

			// Connect to Global signal to know when Player is ready
			if (IsInstanceValid(Global.Instance))
			{
				Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
			}
			else
			{
				GD.PrintErr($"{Name}: Global.Instance is invalid in _Ready. Cannot connect setup signal. Owl might not activate.");
			}

			DeactivateCollisions(); // Start deactivated
			SetPhysicsProcess(false); // Start with physics off until player is confirmed
		}

		public override void _ExitTree()
		{
			// Disconnect signals when removed from the tree
			if (IsInstanceValid(DetectionArea))
			{
				DetectionArea.AreaEntered -= OnDetectionAreaEntered;
				DetectionArea.AreaExited -= OnDetectionAreaExited;
			}

			if (IsInstanceValid(HurtBox))
			{
				HurtBox.AreaEntered -= OnHurtBoxEntered;
			}

			if (IsInstanceValid(CooldownTimer))
			{
				CooldownTimer.Timeout -= OnCooldownTimerTimeout;
			}
			// Disconnect from Global signal
			if (IsInstanceValid(Global.Instance))
			{
				Global.Instance.OnMainNodeSetupFinishedSignal -= OnMainNodeReady;
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			// Check player validity *before* using it for targetting/following
			// If player becomes invalid mid-game, revert to idle hover?
			if (!IsInstanceValid(_cachedPlayer))
			{
				// Player isn't ready or became invalid, maybe hover idly?
				Vector2 hoverVelocity = Vector2.Zero;
				AddSinToMovement(ref hoverVelocity); // Add oscillation even without player
				Velocity = hoverVelocity;
				MoveAndSlide();
				// Ensure target is cleared if player disappears
				if (HasTarget)
				{
					DeactivateTarget();
				}

				return; // Don't proceed with target/follow logic
			}

			// --- Original Physics Logic (Now safe to assume _cachedPlayer is potentially valid) ---
			Vector2 targetVelocity = Vector2.Zero;

			// --- Target Acquisition ---
			if (!HasTarget)
			{
				Target = TryGetMonsterInsideDetectionArea();
				if (!HasTarget)
				{
					ProcessIfNoTarget(ref targetVelocity); // Follow Player or Hover
					AnimationPlayer?.Play(WeaponAnimations.RESET);
				}
				// If target acquired, next block handles it
			}

			// --- Target Handling ---
			if (HasTarget)
			{
				if (!IsInstanceValid(Target)) // Double check target validity
				{
					DeactivateTarget();
					ProcessIfNoTarget(ref targetVelocity); // Revert to follow/hover
					AnimationPlayer?.Play(WeaponAnimations.RESET);
				}
				else // Target is valid
				{
					ProcessIfHasTarget(ref targetVelocity); // Attack or Retreat from Target
					AnimationPlayer?.Play(WeaponAnimations.OnOwlFriendEyesOnAttack);
				}
			}

			// --- Apply Movement ---
			Velocity = targetVelocity;
			MoveAndSlide();
		}

		// --- Private Methods ---

		/// <summary>
		/// Handler for the Global signal indicating main nodes are ready.
		/// Caches the player reference and enables physics processing.
		/// </summary>
		private void OnMainNodeReady()
		{
			if (!IsInstanceValid(this))
			{
				return; // Check if Owl is still valid
			}

			// Now it should be safe to get the Player reference
			if (IsInstanceValid(Global.Instance))
			{
				_cachedPlayer = Global.Instance.Player;
				if (IsInstanceValid(_cachedPlayer))
				{
					GD.Print($"{Name}: Player reference cached via OnMainNodeReady.");
					SetPhysicsProcess(true); // Enable physics processing now that player exists
											 // Start initial animation if needed
					AnimationPlayer?.Play(WeaponAnimations.RESET); // e.g., start idle/follow animation
				}
				else
				{
					GD.PrintErr($"{Name}: OnMainNodeReady called, but Global.Instance.Player is still invalid!");
					SetPhysicsProcess(false); // Keep disabled if player invalid
				}
			}
			else
			{
				GD.PrintErr($"{Name}: OnMainNodeReady called, but Global.Instance is invalid!");
				SetPhysicsProcess(false);
			}
		}

		/// <summary>
		/// Resets the attack ready flag when the cooldown timer finishes.
		/// </summary>
		private void OnCooldownTimerTimeout()
		{
			if (!IsInstanceValid(this))
			{
				return;
			}

			_attackReady = true;
		}

		/// <summary>
		/// Validates that essential exported nodes are assigned.
		/// </summary>
		private bool ValidateExports()
		{
			bool isValid = true;
			if (DetectionArea == null) { GD.PrintErr($"{Name}: Missing DetectionArea!"); isValid = false; }

			if (CooldownTimer == null) { GD.PrintErr($"{Name}: Missing CooldownTimer!"); isValid = false; }

			if (HurtBox == null) { GD.PrintErr($"{Name}: Missing HurtBox!"); isValid = false; }

			if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

			return isValid;
		}

		/// <summary>
		/// Tries to find the first valid EnemyBase within the detection area.
		/// </summary>
		/// <returns>An EnemyBase instance or null if none found.</returns>
		private EnemyBase TryGetMonsterInsideDetectionArea()
		{
			if (!IsInstanceValid(DetectionArea))
			{
				return null;
			}

			foreach (Node areaNode in DetectionArea.GetOverlappingAreas())
			{
				// Check validity of areaNode before getting parent
				if (!IsInstanceValid(areaNode))
				{
					continue;
				}

				// Check if the area is a HitBox and its parent is a valid EnemyBase
				if (areaNode is HitBox hitBox && IsInstanceValid(hitBox.GetParent()) && hitBox.GetParent() is EnemyBase enemy)
				{
					// Optional: Check if enemy is already dead?
					// if (!enemy.IsDead)
					return enemy;
				}
			}

			return null;
		}

		/// <summary>
		/// Handles collision between the Owl's HurtBox and an enemy area.
		/// Starts the attack cooldown.
		/// </summary>
		private void OnHurtBoxEntered(Area2D area2D)
		{
			if (!IsInstanceValid(this) || !IsInstanceValid(area2D))
			{
				return;
			}

			// Check if the colliding area's parent is the current target
			if (HasTarget && IsInstanceValid(area2D.GetParent()) && area2D.GetParent() == Target)
			{
				GD.Print($"{Name} hit target: {Target.Name}");
				_attackReady = false;
				DeactivateCollisions();
				CooldownTimer?.Start();
			}
		}

		/// <summary>
		/// Calculates velocity when the Owl has a valid target.
		/// Moves towards the target if ready to attack, otherwise retreats.
		/// </summary>
		private void ProcessIfHasTarget(ref Vector2 currentVelocity)
		{
			// Target validity is checked before calling this method

			if (_attackReady) // Attack phase: Move towards target
			{
				ActivateCollisions();
				Vector2 direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
				currentVelocity = direction * AttackSpeed;
				LookAt(Target.GlobalPosition);
				Rotation += Mathf.Pi / 2.0f; // Add 90 degrees offset
			}
			else // Cooldown phase: Retreat after attacking
			{
				ProcessOnJustAfterAttack(ref currentVelocity);
			}
		}

		/// <summary>
		/// Calculates velocity when the Owl is in cooldown after attacking.
		/// Moves away from the target if too close, otherwise hovers with oscillation.
		/// </summary>
		private void ProcessOnJustAfterAttack(ref Vector2 currentVelocity)
		{
			// Target validity checked before calling parent method
			LookAtUp();

			if (GlobalPosition.DistanceSquaredTo(Target.GlobalPosition) <= TargetRetreatDistance * TargetRetreatDistance)
			{
				Vector2 direction = Target.GlobalPosition.DirectionTo(GlobalPosition);
				currentVelocity = direction * RegularSpeed;
			}
			else { currentVelocity = Vector2.Zero; }

			AddSinToMovement(ref currentVelocity);
		}

		/// <summary>
		/// Calculates velocity when the Owl has no target.
		/// Moves towards the player if too far, otherwise hovers near player with oscillation.
		/// </summary>
		private void ProcessIfNoTarget(ref Vector2 currentVelocity)
		{
			LookAtUp();

			// Follow player ONLY if they are valid and too far away
			if (IsInstanceValid(_cachedPlayer) &&
				GlobalPosition.DistanceSquaredTo(_cachedPlayer.GlobalPosition) >= PlayerFollowDistance * PlayerFollowDistance)
			{
				Vector2 direction = GlobalPosition.DirectionTo(_cachedPlayer.GlobalPosition);
				currentVelocity = direction * RegularSpeed;
			}
			else { currentVelocity = Vector2.Zero; } // Hover if close or player invalid

			AddSinToMovement(ref currentVelocity);
		}

		/// <summary>
		/// Adds a vertical sinusoidal oscillation to the velocity.
		/// </summary>
		private void AddSinToMovement(ref Vector2 currentVelocity)
		{
			if (OscillationPeriodMs <= 0)
			{
				return;
			}

			float oscillation = OscillationAmplitude * Mathf.Sin(Time.GetTicksUsec() / OscillationPeriodMs * Mathf.Pi * 2.0f);
			currentVelocity.Y += oscillation;
		}

		/// <summary>
		/// Handles an area exiting the detection zone. Clears target if it was the one leaving.
		/// </summary>
		private void OnDetectionAreaExited(Area2D area)
		{
			if (!IsInstanceValid(this) || !IsInstanceValid(area))
			{
				return;
			}

			if (area.GetParent() is EnemyBase characterBody && IsInstanceValid(characterBody))
			{
				if (Target == characterBody) { DeactivateTarget(); }
			}
		}

		/// <summary>
		/// Handles an area entering the detection zone. Acquires target if none exists.
		/// </summary>
		private void OnDetectionAreaEntered(Area2D area)
		{
			if (!IsInstanceValid(this) || HasTarget || !IsInstanceValid(area))
			{
				return;
			}

			if (area.GetParent() is EnemyBase characterBody && IsInstanceValid(characterBody))
			{
				// Optional: Check if enemy is already dead?
				// if (!characterBody.IsDead)
				Target = characterBody;
				GD.Print($"{Name} acquired target: {Target.Name}");
			}
		}

		/// <summary>
		/// Activates necessary collision shapes for attacking/interacting.
		/// </summary>
		private void ActivateCollisions()
		{
			if (IsInstanceValid(HurtBox))
			{
				HurtBox.ActivateCollisionLayer(CollisionLayers.PlayerRegularHurtBox);
				HurtBox.ActivateCollisionMask(CollisionLayers.RegularEnemyHitBox);
			}
			// Activate main body collision?
			// this.ActivateCollisionLayer(CollisionLayers.Player); // Example if needed
		}

		/// <summary>
		/// Deactivates collision shapes.
		/// </summary>
		private void DeactivateCollisions()
		{
			if (IsInstanceValid(HurtBox))
			{
				HurtBox.ResetCollisionLayerAndMask();
			}

			this.ResetCollisionLayerAndMask();
		}

		/// <summary>
		/// Clears the current target.
		/// </summary>
		private void DeactivateTarget()
		{
			if (Target != null)
			{
				GD.Print($"{Name} lost target: {Target.Name}");
				Target = null;
			}
		}

		/// <summary>
		/// Resets the Owl's rotation to face upwards (0 radians).
		/// </summary>
		private void LookAtUp() => Rotation = 0;

	}

	// ==================================================
	// --- Assumed Supporting Code (Place in appropriate files) ---
	// ==================================================
	/* (Assumed code remains the same) */
}
