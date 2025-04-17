using AlfaEBetto.Components;
using Godot;

namespace AlfaEBetto.EnemyWeapons;

public sealed partial class TurrentBase : Area2D
{
	// --- Exports ---
	// Note: Property name uses 'Turrent'. Rename if 'Turret' was intended.
	[Export] public TurrentControllerComponent TurrentControllerComponent { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public Marker2D Muzzle { get; set; }
	[Export] public Godot.Timer CooldownTimer { get; set; }
	[Export(PropertyHint.Range, "0.1, 10.0, 0.1")] // Example range for rotation speed
	public float RotationSpeed { get; set; } = Mathf.Pi / 10.0f;

	// --- Signals ---
	// Emitted when the shoot animation reaches the point where the projectile should spawn.
	[Signal] public delegate void ShootPointReachedSignalEventHandler();

	// --- Private State ---
	private bool _isAllowedToShoot = false;
	private bool _isCooldownTimeoutFinished = true; // Corrected typo: Timout -> Timeout

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			SetProcess(false);
			SetPhysicsProcess(false);
			return;
		}

		// Connect signals using strongly-typed +=
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
		CooldownTimer.Timeout += OnCooldownTimerTimeout; // Corrected method name
	}

	public override void _ExitTree()
	{
		// Disconnect signals when removed from the tree
		if (IsInstanceValid(AnimationPlayer))
		{
			AnimationPlayer.AnimationFinished -= OnAnimationFinished;
		}

		if (IsInstanceValid(CooldownTimer))
		{
			CooldownTimer.Timeout -= OnCooldownTimerTimeout;
		}
	}

	// --- Public Methods ---

	/// <summary>
	/// Allows the turret to shoot when conditions are met.
	/// </summary>
	public void AllowShoot() => _isAllowedToShoot = true;

	/// <summary>
	/// Prevents the turret from shooting.
	/// </summary>
	public void DisallowShoot() => _isAllowedToShoot = false; // Corrected typo: Desallow -> Disallow

	/// <summary>
	/// Attempts to initiate the shooting sequence if allowed and not on cooldown.
	/// </summary>
	public void Shoot()
	{
		// Check permissions and cooldown state
		if (_isAllowedToShoot && _isCooldownTimeoutFinished)
		{
			// Play shooting animation (check validity)
			AnimationPlayer?.Play(EnemyWeaponAnimations.TurrentShoot);
			// Cooldown will start when animation finishes (see OnAnimationFinished)
		}
	}

	/// <summary>
	/// Intended to be called by an Animation Method Track at the exact frame
	/// where the projectile should be spawned by the controller. Emits the signal.
	/// </summary>
	public void OnAnimationShootReady() =>
		// Using nameof() with the delegate name is safe for emitting
		EmitSignal(SignalName.ShootPointReachedSignal);// Or using Godot 4 SignalName: EmitSignal(SignalName.ShootPointReachedSignal);

	// --- Private Methods ---

	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		// TurrentControllerComponent might be optional depending on design? Add check if required.
		// if (TurrentControllerComponent == null) { GD.PrintErr($"{Name}: Missing TurrentControllerComponent!"); isValid = false; }
		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		if (Muzzle == null) { GD.PrintErr($"{Name}: Missing Muzzle (Marker2D)!"); isValid = false; }

		if (CooldownTimer == null) { GD.PrintErr($"{Name}: Missing CooldownTimer!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Resets the cooldown flag when the timer finishes.
	/// </summary>
	private void OnCooldownTimerTimeout() => _isCooldownTimeoutFinished = true;

	/// <summary>
	/// Handles the AnimationFinished signal. Starts the cooldown timer after shooting.
	/// </summary>
	private void OnAnimationFinished(StringName animationName)
	{
		// Check if this instance is still valid
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Start cooldown after the shoot animation finishes
		if (animationName == EnemyWeaponAnimations.TurrentShoot)
		{
			// Check timer validity before starting
			if (IsInstanceValid(CooldownTimer))
			{
				CooldownTimer.Start();
				_isCooldownTimeoutFinished = false; // Immediately set cooldown state
			}
			else
			{
				GD.PrintErr($"{Name}: CooldownTimer is invalid, cannot start cooldown.");
				// Maybe set _isCooldownTimeoutFinished = true immediately as a fallback?
				_isCooldownTimeoutFinished = true;
			}
		}
	}
}
