
using System;
using Godot;

public sealed partial class HeartShieldUi : Control
{
	// --- Exports ---
	[Export] public ProgressBar HealthProgressBar { get; set; }
	[Export] public ProgressBar ShieldProgressBar { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public AudioStreamPlayer HeartBeatAudioStreamPlayer { get; set; }

	[ExportGroup("Configuration")]
	[Export(PropertyHint.Range, "0.01, 1.0, 0.01")] // Use float range for threshold
	public float HealthBlinkThreshold { get; set; } = 0.30f;
	[Export(PropertyHint.Range, "0.1, 5.0, 0.1")] // Use float range for speed multiplier
	public float MaxBlinkSpeed { get; set; } = 2f;

	// --- Properties ---
	// Access Global Player safely
	private Player _player => Global.Instance?.Player; // Use null-conditional access

	// --- Godot Methods ---

	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating.");
			QueueFree(); // Or SetProcess(false), SetPhysicsProcess(false)
			return;
		}

		// Assuming SetVisibilityZOrdering extension method exists
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);

		// Connect to Global singleton signal - MUST be disconnected in _ExitTree
		// Check if Instance exists before connecting
		if (Global.Instance != null)
		{
			// Using += is strongly typed if Global defines the delegate
			Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
		}
		else
		{
			GD.PrintErr($"{Name}: Global.Instance is null in _Ready. Cannot connect setup signal.");
		}

		// Setup heartbeat looping
		// Check instance validity before connecting
		if (IsInstanceValid(HeartBeatAudioStreamPlayer))
		{
			HeartBeatAudioStreamPlayer.Finished += OnHeartbeatFinished;
		}
	}

	public override void _ExitTree()
	{
		// --- CRITICAL: Disconnect all signals connected to external or persistent nodes ---
		if (Global.Instance != null)
		{
			// Check if signal exists before disconnecting (optional safety)
			// Using the delegate name directly with -= is the strongly-typed way
			Global.Instance.OnMainNodeSetupFinishedSignal -= OnMainNodeReady;
		}

		// Disconnect from player signals
		DisconnectPlayerSignals();

		// Disconnect from local nodes
		if (IsInstanceValid(HeartBeatAudioStreamPlayer))
		{
			HeartBeatAudioStreamPlayer.Finished -= OnHeartbeatFinished;
		}
	}

	// --- Signal Handlers ---

	/// <summary>
	/// Called via Global signal when Player and Stage are ready.
	/// Sets up initial progress bar values and connects to Player signals.
	/// </summary>
	private void OnMainNodeReady()
	{
		// --- FIX: Check if this instance itself is still valid ---
		// This prevents executing code if the handler is called on a disposed object
		// due to signal timing issues during scene transitions.
		if (!IsInstanceValid(this))
		{
			// GD.Print($"DEBUG: OnMainNodeReady called on disposed instance: {this}"); // Optional debug print
			return; // Exit immediately if the object is disposed
		}
		// ----------------------------------------------------------

		// Ensure _player reference is valid before proceeding
		Player currentPlayer = _player; // Get player instance via property
		if (!IsInstanceValid(currentPlayer))
		{
			GD.PrintErr($"{Name}: OnMainNodeReady called, but Global.Instance.Player is null or invalid.");
			// Maybe hide UI or set default state?
			if (IsInstanceValid(HealthProgressBar))
			{
				HealthProgressBar.Value = HealthProgressBar.MinValue;
			}

			if (IsInstanceValid(ShieldProgressBar))
			{
				ShieldProgressBar.Value = ShieldProgressBar.MinValue;
				ShieldProgressBar.Visible = false;
			}

			return;
		}

		// Now safe to proceed, log uses 'Name' which requires valid instance
		GD.Print($"{Name}: Handling OnMainNodeReady.");

		// --- Setup Progress Bars ---
		// Ensure components exist before accessing properties
		if (IsInstanceValid(currentPlayer.HealthComponent))
		{
			SetProgressBar(
				HealthProgressBar,
				currentPlayer.HealthComponent.MaxHealth,
				currentPoints: currentPlayer.HealthComponent.CurrentHealth, // Use current health
				minValue: 0,
				exp: false
			);
			// Connect health change signal (ensure previous connections are cleared if necessary - handled in DisconnectPlayerSignals)
			currentPlayer.HealthComponent.OnHealthChangedSignal -= OnHealthChange; // Prevent duplicates
			currentPlayer.HealthComponent.OnHealthChangedSignal += OnHealthChange;
		}
		else
		{
			GD.PrintErr($"{Name}: Player HealthComponent is null or invalid in OnMainNodeReady.");
			// Set health bar to default/empty state
			SetProgressBar(HealthProgressBar, 100, 0);
		}

		if (IsInstanceValid(currentPlayer.PlayerShield))
		{
			SetProgressBar(
				ShieldProgressBar,
				currentPlayer.PlayerShield.MaxShieldPoints,
				currentPoints: currentPlayer.PlayerShield.CurrentShieldPoints
			);
			ShieldProgressBar.Visible = currentPlayer.PlayerShield.IsActive && currentPlayer.PlayerShield.CurrentShieldPoints > 0;
			// Connect shield change signal (ensure previous connections are cleared if necessary - handled in DisconnectPlayerSignals)
			currentPlayer.PlayerShield.OnShieldPointsChangedSignal -= OnShieldChanged; // Prevent duplicates
			currentPlayer.PlayerShield.OnShieldPointsChangedSignal += OnShieldChanged;
		}
		else
		{
			GD.PrintErr($"{Name}: Player PlayerShield is null or invalid in OnMainNodeReady.");
			// Hide shield bar
			ShieldProgressBar.Visible = false;
		}

		// Stop heartbeat initially (will start if health drops)
		HeartBeatAudioStreamPlayer?.Stop();
	}

	/// <summary>
	/// Disconnects signals previously connected to player components.
	/// Important to call this before the player instance becomes invalid.
	/// </summary>
	private void DisconnectPlayerSignals()
	{
		// Access player via the property which checks Global.Instance
		// Note: _player might be null here if Global.Instance or Global.Instance.Player is null/invalid
		Player currentPlayer = _player;
		// We only need to check if currentPlayer *was* valid to potentially disconnect
		if (IsInstanceValid(currentPlayer)) // Check if the player instance we *might* have connected to is still valid
		{
			// Check components before disconnecting
			if (IsInstanceValid(currentPlayer.HealthComponent))
			{
				currentPlayer.HealthComponent.OnHealthChangedSignal -= OnHealthChange;
			}

			if (IsInstanceValid(currentPlayer.PlayerShield))
			{
				currentPlayer.PlayerShield.OnShieldPointsChangedSignal -= OnShieldChanged;
			}
		}
		// If currentPlayer is not valid, we assume signals are already implicitly disconnected or irrelevant.
	}

	/// <summary>
	/// Loops the heartbeat sound.
	/// </summary>
	private void OnHeartbeatFinished()
	{
		// Check validity before playing again
		if (IsInstanceValid(HeartBeatAudioStreamPlayer) && HeartBeatAudioStreamPlayer.Playing) // Check if it *should* be playing (e.g., low health)
		{
			// The logic in OnHealthChange now handles restarting the player if needed.
			// Avoid unconditionally restarting here, it might override the Stop() call.
			// HeartBeatAudioStreamPlayer.Play();
		}
	}

	/// <summary>
	/// Updates the shield progress bar display.
	/// </summary>
	private void OnShieldChanged(int currentShieldPoints, bool isIncrease)
	{
		// Add validity check for 'this' instance as well, as it's a signal handler
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Check validity of the progress bar node
		if (!IsInstanceValid(ShieldProgressBar))
		{
			return;
		}

		ShieldProgressBar.Value = currentShieldPoints;
		// Hide shield progress bar if shield points are zero or less
		ShieldProgressBar.Visible = currentShieldPoints > 0;
	}

	/// <summary>
	/// Updates the health progress bar and handles low-health effects (blink, heartbeat).
	/// </summary>
	private void OnHealthChange(int currentHealth, bool isIncrease)
	{
		// Add validity check for 'this' instance as well, as it's a signal handler
		if (!IsInstanceValid(this))
		{
			return;
		}

		// Check validity of the progress bar node
		if (!IsInstanceValid(HealthProgressBar))
		{
			return;
		}

		HealthProgressBar.Value = currentHealth;

		// Ensure MaxValue is positive to avoid division by zero or incorrect logic
		if (HealthProgressBar.MaxValue <= 0)
		{
			return;
		}

		// Calculate health ratio safely
		double healthRatio = HealthProgressBar.Value / HealthProgressBar.MaxValue;

		// Check if health is below the threshold
		if (healthRatio <= HealthBlinkThreshold)
		{
			// Calculate blink speed based on how low the health is (lower health = faster blink)
			// Ensure blinkSpeedUp doesn't become zero or negative if ratio is exactly threshold
			double blinkSpeedUp = Math.Max(0.1, MaxBlinkSpeed * (1.0 - (healthRatio / HealthBlinkThreshold))); // Scale within the threshold range

			AnimationPlayer?.Play(UiAnimations.OnHealthDanger, customSpeed: (float)blinkSpeedUp);

			if (IsInstanceValid(HeartBeatAudioStreamPlayer))
			{
				HeartBeatAudioStreamPlayer.PitchScale = (float)blinkSpeedUp;
				// Only Play() if it's not already playing to avoid restarting constantly
				if (!HeartBeatAudioStreamPlayer.Playing)
				{
					HeartBeatAudioStreamPlayer.Play();
				}
			}
		}
		else // Health is above threshold
		{
			// Reset animation and stop heartbeat
			AnimationPlayer?.Play(UiAnimations.RESET); // Play idle/reset animation
			HeartBeatAudioStreamPlayer?.Stop();
		}
	}

	// --- Helpers ---

	private bool ValidateExports()
	{
		bool isValid = true;
		if (HealthProgressBar == null) { GD.PrintErr($"{Name}: Missing HealthProgressBar!"); isValid = false; }

		if (ShieldProgressBar == null) { GD.PrintErr($"{Name}: Missing ShieldProgressBar!"); isValid = false; }

		if (AnimationPlayer == null) { GD.PrintErr($"{Name}: Missing AnimationPlayer!"); isValid = false; }

		if (HeartBeatAudioStreamPlayer == null) { GD.PrintErr($"{Name}: Missing HeartBeatAudioStreamPlayer!"); isValid = false; }

		return isValid;
	}

	/// <summary>
	/// Configures a ProgressBar's basic properties.
	/// </summary>
	private void SetProgressBar(ProgressBar progressBar, int maxValue, int? currentPoints = null, int minValue = 0, bool exp = false)
	{
		// Check validity before accessing properties
		if (!IsInstanceValid(progressBar))
		{
			GD.PrintErr($"Attempted to set properties on an invalid ProgressBar instance in {Name}.");
			return;
		}

		progressBar.MinValue = minValue; // Set min before max/value
		progressBar.MaxValue = Math.Max(minValue, maxValue); // Ensure max >= min
															 // Ensure value is within range [min, max]
		progressBar.Value = Mathf.Clamp(currentPoints ?? maxValue, (int)progressBar.MinValue, (int)progressBar.MaxValue);
		progressBar.Rounded = true;
		progressBar.ShowPercentage = false;
		progressBar.ExpEdit = exp;
	}
}
