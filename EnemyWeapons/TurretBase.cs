using Alfaebeto.Components; // Assuming TurretControllerComponent is here
using AlfaEBetto.EnemyWeapons;
using Godot;
// Assuming EnemyWeaponAnimations is accessible
// using Alfaebeto.EnemyWeapons;

namespace Alfaebeto.EnemyWeapons; // Corrected namespace

/// <summary>
/// Base logic for an enemy turret. Handles aiming permissions, cooldown,
/// playing shoot animations, and signaling the exact point for projectile spawn via animation.
/// Assumes an external controller calls Shoot() and connects to ShootPointReachedSignal.
/// Changed base to Node2D assuming no area detection is needed for the base itself.
/// </summary>
public sealed partial class TurretBase : Node2D // Changed base class
{
	#region Exports
	/// <summary>
	/// Optional controller component (often on the same node or parent)
	/// that handles aiming logic and calls this turret's Shoot() method.
	/// </summary>
	[Export] public TurretControllerComponent TurretControllerComponent { get; set; }

	/// <summary>
	/// AnimationPlayer for shooting animations. Assign in Inspector.
	/// </summary>
	[Export] public AnimationPlayer AnimationPlayerNode { get; set; } // Renamed export

	/// <summary>
	/// Marker2D indicating the projectile spawn point and initial direction. Assign in Inspector.
	/// </summary>
	[Export] public Marker2D Muzzle { get; set; }

	/// <summary>
	/// Timer node managing the cooldown between shots. Assign in Inspector.
	/// </summary>
	[Export] public Godot.Timer CooldownTimer { get; set; }

	//RotationSpeed seems unused in this script, maybe used by TurretControllerComponent?
	[Export(PropertyHint.Range, "0.1, 10.0, 0.1")]
	public float RotationSpeed { get; set; } = Mathf.Pi / 10.0f;
	#endregion

	#region Signals
	/// <summary>
	/// Emitted by the shoot animation via OnAnimationShootReady() when the
	/// projectile should be spawned. Connect to the component responsible for spawning.
	/// </summary>
	[Signal] public delegate void ShootPointReachedSignalEventHandler();
	#endregion

	#region Private State
	private bool _isAllowedToShoot = false;
	private bool _isCooldownFinished = true; // Renamed for clarity
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		if (!ValidateExports())
		{
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes. Deactivating.");
			SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		ConnectSignals();
		_isInitialized = true;
	}

	public override void _ExitTree()
	{
		DisconnectSignals();
		CooldownTimer?.Stop(); // Stop timer on exit
	}
	#endregion

	#region Public Control Methods
	public void AllowShoot() =>
		// GD.Print($"{Name}: AllowShoot set to true.");
		_isAllowedToShoot = true;

	public void DisallowShoot() // Corrected typo
=>
		// GD.Print($"{Name}: AllowShoot set to false.");
		_isAllowedToShoot = false;

	/// <summary>
	/// Attempts to initiate the shooting sequence by playing the shoot animation,
	/// if allowed and not on cooldown. Called by an external controller (e.g., TurretControllerComponent).
	/// </summary>
	public void Shoot()
	{
		if (!_isInitialized)
		{
			return;
		}

		// GD.Print($"{Name}: Shoot() called. Allowed={_isAllowedToShoot}, CooldownFinished={_isCooldownFinished}"); // Debug print

		if (_isAllowedToShoot && _isCooldownFinished)
		{
			// GD.Print($"{Name}: Playing shoot animation."); // Debug print
			// Assuming animation constant typo fixed: Turrent->Turret
			AnimationPlayerNode?.Play(EnemyWeaponAnimations.TurretShoot);
			// Cooldown timer is started by OnAnimationFinished
		}
	}

	/// <summary>
	/// **MUST BE CALLED BY A 'Call Method Track' IN THE SHOOT ANIMATION.**
	/// Emits the signal indicating the precise moment to spawn the projectile.
	/// </summary>
	public void OnAnimationShootReady() =>
		// GD.Print($"{Name}: Animation reached shoot point, emitting signal."); // Debug print
		EmitSignal(SignalName.ShootPointReachedSignal);
	#endregion

	#region Signal Handlers & Private Methods
	private void ConnectSignals()
	{
		// Validation ensures nodes are not null here
		AnimationPlayerNode.AnimationFinished += OnAnimationFinished;
		CooldownTimer.Timeout += OnCooldownTimerTimeout;
	}

	private void DisconnectSignals()
	{
		if (IsInstanceValid(AnimationPlayerNode))
		{
			AnimationPlayerNode.AnimationFinished -= OnAnimationFinished;
		}

		if (IsInstanceValid(CooldownTimer))
		{
			CooldownTimer.Timeout -= OnCooldownTimerTimeout;
		}
	}

	private bool ValidateExports()
	{
		bool isValid = true;
		void CheckNode(Node node, string name)
		{
			if (node == null) { GD.PrintErr($"{Name} ({GetPath()}): Exported node '{name}' is null!"); isValid = false; }
		}
		// Check if controller is actually required for this node's function
		// CheckNode(TurretControllerComponent, nameof(TurretControllerComponent));
		CheckNode(AnimationPlayerNode, nameof(AnimationPlayerNode));
		CheckNode(Muzzle, nameof(Muzzle));
		CheckNode(CooldownTimer, nameof(CooldownTimer));
		return isValid;
	}

	/// <summary>
	/// Resets the cooldown flag when the timer finishes.
	/// </summary>
	private void OnCooldownTimerTimeout() =>
		// GD.Print($"{Name}: Cooldown finished."); // Debug print
		_isCooldownFinished = true;

	/// <summary>
	/// Handles the AnimationFinished signal. Starts the cooldown timer after shooting.
	/// </summary>
	private void OnAnimationFinished(StringName animationName)
	{
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Start cooldown AFTER the shoot animation finishes
		// Assuming animation constant typo fixed: Turrent->Turret
		if (animationName == EnemyWeaponAnimations.TurretShoot)
		{
			if (IsInstanceValid(CooldownTimer))
			{
				// GD.Print($"{Name}: Shoot animation finished, starting cooldown."); // Debug print
				CooldownTimer.Start();
				_isCooldownFinished = false; // Immediately set cooldown state
			}
			else
			{
				GD.PrintErr($"{Name} ({GetPath()}): CooldownTimer is invalid, cannot start cooldown.");
				_isCooldownFinished = true; // Fallback: assume ready if timer broken? Or stay false? Needs decision.
			}
		}
	}
	#endregion
}
