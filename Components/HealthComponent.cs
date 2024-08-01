using Godot;

public sealed partial class HealthComponent : Node
{
	[Export]
	public int MaxHealth { get; set; } = 100;
	[Export]
	public bool EmmitInBetweenSignals { get; set; } = false;
	[Export]
	// The higher the lower is health
	public int HeathLevelSignalsIntervals { get; set; } = 1;
	public int CurrentHealth { get; private set; }

	private int _currentLevel = 0;

	[Signal]
	public delegate void OnHealthChangedSignalEventHandler(int currentHealth);
	[Signal]
	public delegate void OnHealthDepletedSignalEventHandler();
	[Signal]
	public delegate void OnHealthLevelChangeSignalEventHandler(int percentageLeft);

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		_currentLevel = HeathLevelSignalsIntervals;
	}

	public void TakeDamage(int damage)
	{
		CurrentHealth -= damage;
		_ = EmitSignal(nameof(OnHealthChangedSignal), CurrentHealth);
		if (CurrentHealth <= 0)
		{
			_ = EmitSignal(nameof(OnHealthDepletedSignal));
		}
		else if (EmmitInBetweenSignals)
		{
			CalculateLevel();
		}
	}

	public void Heal(int healAmount)
	{
		CurrentHealth += healAmount;
		if (CurrentHealth > MaxHealth)
		{
			CurrentHealth = MaxHealth;
		}

		_ = EmitSignal(nameof(OnHealthChangedSignal), CurrentHealth);
		if (EmmitInBetweenSignals)
		{
			CalculateLevel();
		}
	}

	public void CalculateLevel()
	{
		float relatvieLevel = (MaxHealth - CurrentHealth) / (float)MaxHealth;
		int new_level = Mathf.FloorToInt(HeathLevelSignalsIntervals * relatvieLevel);
		if (new_level != _currentLevel)
		{
			_ = EmitSignal(nameof(OnHealthLevelChangeSignal), new_level);
			_currentLevel = Mathf.Min(new_level, HeathLevelSignalsIntervals - 1);
		}
	}
}

