using AlfaEBetto.CustomNodes;
using Godot;
// using Alfaebeto.Extensions; // Only if specific extensions are used

namespace Alfaebeto.Components; // Corrected namespace

/// <summary>
/// Manages the "hurt" state of an entity, providing a cooldown period
/// after being hit via an associated HitBox. Emits a signal when hurt occurs.
/// </summary>
public sealed partial class HurtComponent : Node
{
	#region Exports
	/// <summary>
	/// The Timer node used for the hurt cooldown period.
	/// </summary>
	[Export] public Timer HurtCooldownTimer { get; set; }

	/// <summary>
	/// The HitBox (Area2D) that detects incoming hits.
	/// </summary>
	[Export] public HitBox HitBox { get; set; }
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

	// Removed unused HitEnemyVelocity property
	// public Vector2 HitEnemyVelocity { get; private set; } = Vector2.Zero;
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. HurtComponent will not function correctly.");
			SetProcess(false); // Deactivate if setup fails
			SetPhysicsProcess(false);
			return;
		}

		ConnectSignals();
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
		HurtCooldownTimer.Timeout += OnHurtCooldownTimeout; // Renamed handler
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

	/// <summary>
	/// Called when an Area2D enters the associated HitBox.
	/// Triggers the hurt state if not already hurt.
	/// </summary>
	private void OnHitBoxAreaEntered(Area2D area)
	{
		// Ignore hits if already in hurt cooldown
		if (IsHurt)
		{
			return;
		}

		// Enter hurt state
		IsHurt = true;
		HurtCooldownTimer?.Start(); // Start cooldown (null check just in case, though validated)

		// Emit the signal, passing the area that triggered the hit
		EmitSignal(SignalName.OnHurtSignal, area);

		// --- Optional: Add logic here if the component itself should react visually/audibly ---
		// Example: GetParent<Node2D>()?.Modulate = Colors.Red;
		// Example: GetNode<AudioStreamPlayer>("HurtSound")?.Play();
		// Typically, the node OWNING this component connects to OnHurtSignal to handle effects.
	}

	/// <summary>
	/// Called when the HurtCooldownTimer times out, ending the hurt state.
	/// </summary>
	public void OnHurtCooldownTimeout() => IsHurt = false;// --- Optional: Reset visual/audio effects started in OnHitBoxAreaEntered ---// Example: GetParent<Node2D>()?.Modulate = Colors.White;

	#endregion

	#region Validation
	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		if (HurtCooldownTimer == null)
		{
			GD.PrintErr($"{Name}: Exported node '{nameof(HurtCooldownTimer)}' is not assigned.");
			isValid = false;
		}

		if (HitBox == null)
		{
			GD.PrintErr($"{Name}: Exported node '{nameof(HitBox)}' is not assigned.");
			isValid = false;
		}

		return isValid;
	}
	#endregion

	#region Public Methods (Optional)
	/// <summary>
	/// Manually applies the hurt effect, starting the cooldown and emitting the signal.
	/// Useful for damage sources that don't use Area2D collision (e.g., instant damage zones, commands).
	/// </summary>
	/// <param name="sourceArea">Optional: The Area2D that caused the hurt, if applicable.</param>
	public void ApplyHurt(Area2D sourceArea = null) // Added optional method
	{
		// Use the same logic as OnHitBoxAreaEntered
		if (IsHurt)
		{
			return;
		}

		IsHurt = true;
		HurtCooldownTimer?.Start();
		EmitSignal(SignalName.OnHurtSignal, sourceArea); // Pass null if sourceArea is unknown
	}
	#endregion
}