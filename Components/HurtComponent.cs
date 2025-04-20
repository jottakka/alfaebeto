using AlfaEBetto.CustomNodes;
using Godot;

namespace Alfaebeto.Components; // Corrected namespace

/// <summary>
/// Manages the "hurt" state of an entity, providing a cooldown period
/// after being hit via an associated HitBox. Emits a signal when hurt occurs.
/// </summary>
public sealed partial class HurtComponent : Node
{
	#region Exports
	/// <summary>
	/// The Timer node used for the hurt cooldown period. Assign in Inspector.
	/// </summary>
	[Export] public Timer HurtCooldownTimer { get; set; }

	/// <summary>
	/// The HitBox (Area2D) that detects incoming hits. Assign in Inspector.
	/// </summary>
	[Export] public HitBox HitBox { get; set; }

	/// <summary>
	/// The duration (in seconds) of the hurt cooldown period after being hit.
	/// </summary>
	[Export(PropertyHint.Range, "0.0, 5.0, 0.05")] // Example range: 0 to 5 secs, step 0.05
	public double CooldownSeconds { get; set; } = 0.5; // Default cooldown time
	#endregion

	#region Signals
	/// <summary>
	/// Emitted when the associated HitBox detects a valid hit and the component
	/// is not already in the hurt state (i.e., cooldown is not active).
	/// Passes the Area2D that entered the HitBox.
	/// </summary>
	[Signal] public delegate void OnHurtSignalEventHandler(Area2D triggeringArea);
	#endregion

	#region State
	/// <summary>
	/// Gets a value indicating whether the entity is currently in the "hurt" state (cooldown active).
	/// </summary>
	public bool IsHurt { get; private set; } = false;
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
			GD.PrintErr($"{Name} ({GetPath()}): Missing required exported nodes. HurtComponent will not function correctly.");
			SetProcess(false); // Deactivate if setup fails
			SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		// --- Configure Timer ---
		// Set the timer's wait time from the exported variable
		HurtCooldownTimer.WaitTime = CooldownSeconds;
		// Ensure the timer only runs once per Start() call
		HurtCooldownTimer.OneShot = true;
		// ----------------------

		ConnectSignals();
		_isInitialized = true;
	}

	public override void _ExitTree()
	{
		DisconnectSignals();
		// Stop timer if node exits prematurely
		HurtCooldownTimer?.Stop();
	}
	#endregion

	#region Signal Handling & Logic

	private void ConnectSignals()
	{
		// Null checks performed by ValidateExports
		HurtCooldownTimer.Timeout += OnHurtCooldownTimeout;
		HitBox.AreaEntered += OnHitBoxAreaEntered;
	}

	private void DisconnectSignals()
	{
		if (IsInstanceValid(HurtCooldownTimer))
		{
			HurtCooldownTimer.Timeout -= OnHurtCooldownTimeout;
		}

		if (IsInstanceValid(HitBox))
		{
			HitBox.AreaEntered -= OnHitBoxAreaEntered;
		}
	}

	private void OnHitBoxAreaEntered(Area2D area)
	{
		if (IsHurt || !_isInitialized)
		{
			return; // Ignore if already hurt or not initialized
		}

		IsHurt = true;
		// Timer WaitTime is now set in _Ready from CooldownSeconds export
		HurtCooldownTimer.Start(); // Start the configured cooldown

		EmitSignal(SignalName.OnHurtSignal, area);
	}

	public void OnHurtCooldownTimeout() => IsHurt = false;

	#endregion

	#region Validation
	private bool ValidateExports()
	{
		bool isValid = true;
		if (HurtCooldownTimer == null)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Exported node '{nameof(HurtCooldownTimer)}' is not assigned.");
			isValid = false;
		}

		if (HitBox == null)
		{
			GD.PrintErr($"{Name} ({GetPath()}): Exported node '{nameof(HitBox)}' is not assigned.");
			isValid = false;
		}
		// Optional: Validate CooldownSeconds value
		if (CooldownSeconds < 0)
		{
			GD.PushWarning($"{Name} ({GetPath()}): {nameof(CooldownSeconds)} is negative ({CooldownSeconds}). Clamping to 0.");
			CooldownSeconds = 0;
		}

		return isValid;
	}
	#endregion

	#region Public Methods (Optional)
	public void ApplyHurt(Area2D sourceArea = null)
	{
		if (IsHurt || !_isInitialized)
		{
			return;
		}

		IsHurt = true;
		// Timer WaitTime is now set in _Ready from CooldownSeconds export
		HurtCooldownTimer?.Start(); // Use ?. just in case initialization failed partially
		EmitSignal(SignalName.OnHurtSignal, sourceArea);
	}
	#endregion
}