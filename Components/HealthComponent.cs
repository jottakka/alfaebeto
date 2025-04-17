using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.Components;
// Assuming Player class exists in the global namespace or relevant using statement is added
// using YourProject.Actors;

public sealed partial class HealthComponent : Node
{
	// --- Exports ---
	[Export(PropertyHint.Range, "1,10000,1")] // Ensure MaxHealth is at least 1
	public int MaxHealth { get; set; } = 100;

	[Export]
	public bool EmmitInBetweenSignals { get; set; } = false; // Corrected typo: Emmit -> Emit

	[Export(PropertyHint.Range, "1,10,1")] // Define how many "levels" of health depletion to signal
										   // Comment clarified: Level 0 = Full health, Level (Intervals-1) = Low health
	public int HealthLevelSignalsIntervals { get; set; } = 1; // Corrected typo: Heath -> Health

	// --- Signals ---
	[Signal]
	public delegate void OnHealthChangedSignalEventHandler(int currentHealth, bool isIncrease);
	[Signal]
	public delegate void OnHealthDepletedSignalEventHandler();
	[Signal]
	public delegate void OnHealthLevelChangeSignalEventHandler(int currentLevel); // Changed param name for clarity

	// --- Properties ---
	public bool IsDead => CurrentHealth <= 0;
	public int CurrentHealth { get; private set; }

	// --- Private Fields ---
	private int _currentLevel = 0; // Represents the current "damage" level (0 = full health)
	private bool _parentIsPlayer;

	// --- Godot Methods ---

	public override void _Ready()
	{
		// Check parent type once
		_parentIsPlayer = GetParent() is Player;
		// Initialize health and level
		CurrentHealth = MaxHealth;
		// Level 0 represents full health based on CalculateLevel logic
		_currentLevel = 0;
		// Emit initial state signals if needed (optional)
		// EmitStateSignals(true); // Indicate initial state as an "increase" from nothing
	}

	// --- Public Methods ---

	/// <summary>
	/// Applies damage to the component, clamps health, and emits relevant signals.
	/// Handles special case for Player health minimum.
	/// </summary>
	/// <param name="damage">The amount of damage to apply (should be positive).</param>
	public void TakeDamage(int damage)
	{
		// Ignore damage if already dead or damage is non-positive
		if (IsDead || damage <= 0)
		{
			return;
		}

		int previousHealth = CurrentHealth;

		// Special case for player at 1 HP taking any damage
		if (_parentIsPlayer && CurrentHealth == 1)
		{
			CurrentHealth = 0; // Directly set to 0
							   // Emit signals *after* state change
			EmitStateSignals(isIncrease: false); // Reports change 1->0 and calculates level
			EmitSignal(SignalName.OnHealthDepletedSignal);
			return; // Exit early
		}

		// Normal damage calculation
		CurrentHealth -= damage;

		// Determine minimum health value based on parent type
		int minValue = _parentIsPlayer ? 1 : 0;
		CurrentHealth = Mathf.Clamp(CurrentHealth, minValue, MaxHealth);

		// Emit signals only if health actually changed
		if (CurrentHealth != previousHealth)
		{
			EmitStateSignals(isIncrease: false);

			// Check for depletion *after* emitting change signals
			// (Player depletion handled in the special case above)
			if (!_parentIsPlayer && CurrentHealth <= 0) // Check against 0 for depletion
			{
				EmitSignal(SignalName.OnHealthDepletedSignal);
			}
		}
	}

	/// <summary>
	/// Applies healing to the component, clamps health, and emits relevant signals.
	/// </summary>
	/// <param name="healAmount">The amount of health to restore (should be positive).</param>
	public void Heal(int healAmount)
	{
		// Ignore healing if already at max health, dead, or heal amount is non-positive
		if (CurrentHealth == MaxHealth || IsDead || healAmount <= 0)
		{
			return;
		}

		int previousHealth = CurrentHealth;

		CurrentHealth += healAmount;
		// Clamp health between 0 (or 1 for player if relevant, though healing usually starts above 0) and MaxHealth
		int minValue = 0; // Can heal up from 0
		CurrentHealth = Mathf.Clamp(CurrentHealth, minValue, MaxHealth);

		// Emit signals only if health actually changed
		if (CurrentHealth != previousHealth)
		{
			EmitStateSignals(isIncrease: true);
		}
	}

	// --- Private Helpers ---

	/// <summary>
	/// Emits the OnHealthChangedSignal and potentially OnHealthLevelChangeSignal.
	/// </summary>
	/// <param name="isIncrease">True if health increased, false if decreased.</param>
	private void EmitStateSignals(bool isIncrease) // Corrected typo: Sinals -> Signals
	{
		EmitSignal(SignalName.OnHealthChangedSignal, CurrentHealth, isIncrease);

		if (EmmitInBetweenSignals)
		{
			CalculateAndEmitLevel(); // Renamed for clarity
		}
	}

	/// <summary>
	/// Calculates the current health level based on damage taken and emits
	/// OnHealthLevelChangeSignal if the level has changed since the last check.
	/// Level 0 = Full health, Level (Intervals-1) = Low health.
	/// </summary>
	private void CalculateAndEmitLevel()
	{
		// Prevent division by zero if MaxHealth is invalid
		if (MaxHealth <= 0)
		{
			return;
		}
		// Prevent issues if intervals is invalid
		if (HealthLevelSignalsIntervals <= 0)
		{
			return;
		}

		// Calculate relative damage (0.0 = full health, ~1.0 = empty)
		// Ensure float division
		float relativeDamage = (float)(MaxHealth - CurrentHealth) / MaxHealth;
		// Clamp between 0 and 1 just in case CurrentHealth went slightly out of bounds before clamp
		relativeDamage = Mathf.Clamp(relativeDamage, 0.0f, 1.0f);

		// Calculate the corresponding level index (0 to Intervals-1)
		int newLevel = Mathf.FloorToInt(HealthLevelSignalsIntervals * relativeDamage);

		// Clamp level to the maximum possible index (Intervals - 1)
		// This handles the edge case where relativeDamage is exactly 1.0
		newLevel = Mathf.Min(newLevel, HealthLevelSignalsIntervals - 1);

		// Emit signal only if the calculated level is different from the current one
		if (newLevel != _currentLevel)
		{
			_currentLevel = newLevel;
			EmitSignal(SignalName.OnHealthLevelChangeSignal, _currentLevel);
		}
	}
}
