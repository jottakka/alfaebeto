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

	private bool _parentIsPlayer;

	[Signal]
	public delegate void OnHealthChangedSignalEventHandler(int currentHealth, bool isIncrease);
	[Signal]
	public delegate void OnHealthDepletedSignalEventHandler();
	[Signal]
	public delegate void OnHealthLevelChangeSignalEventHandler(int percentageLeft);

	public override void _Ready()
	{
		_parentIsPlayer = GetParent() is Player;
		CurrentHealth = MaxHealth;
		_currentLevel = HeathLevelSignalsIntervals;
	}

	public void TakeDamage(int damage)
	{
		if (_parentIsPlayer && CurrentHealth == 1)
		{
			CurrentHealth = 0;
			_ = EmitSignal(nameof(OnHealthDepletedSignal));
			EmitStateSinals(isIncrease: false);
			return;
		}

		CurrentHealth -= damage;
		int minValeu = _parentIsPlayer ? 1 : 0;
		_ = Mathf.Clamp(CurrentHealth, minValeu, MaxHealth);

		if (CurrentHealth == 0)
		{
			_ = EmitSignal(nameof(OnHealthDepletedSignal));

		}
		else
		{
			EmitStateSinals(isIncrease: false);
		}
	}

	public void Heal(int healAmount)
	{
		if (CurrentHealth == MaxHealth)
		{
			return;
		}

		CurrentHealth += healAmount;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		EmitStateSinals(true);
	}

	private void EmitStateSinals(bool isIncrease)
	{
		_ = EmitSignal(nameof(OnHealthChangedSignal), CurrentHealth, isIncrease);

		if (EmmitInBetweenSignals)
		{
			CalculateLevel();
		}
	}

	private void CalculateLevel()
	{
		float relatvieLevel = (MaxHealth - CurrentHealth) / (float)MaxHealth;
		int new_level = Mathf.FloorToInt(HeathLevelSignalsIntervals * relatvieLevel);
		if (new_level != _currentLevel)
		{
			_currentLevel = Mathf.Min(new_level, HeathLevelSignalsIntervals - 1);
			_ = EmitSignal(nameof(OnHealthLevelChangeSignal), _currentLevel);
		}
	}
}

