using Godot;
public sealed partial class HeartShieldUi : Control
{
	[Export]
	public ProgressBar HealthProgressBar { get; set; }
	[Export]
	public ProgressBar ShieldProgressBar { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public float HealthBlinkThreshold { get; set; } = 0.30f;
	[Export]
	public float MaxBlinkSpeed { get; set; } = 3.0f;

	private Player _player => Global.Instance.Player;

	public override void _Ready()
	{
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
	}

	private void OnMainNodeReady()
	{
		SetProgressBar(
			HealthProgressBar,
			_player.HealthComponent.MaxHealth,
			minValue: 0,
			exp: false
		);
		SetProgressBar(
			ShieldProgressBar,
			_player.PlayerShield.MaxShieldPoints,
			currentPoints: _player.PlayerShield.CurrentShieldPoints
		);
		ShieldProgressBar.Visible = _player.PlayerShield.IsActive;

		_player.HealthComponent.OnHealthChangedSignal += OnHealthChange;
		_player.PlayerShield.OnShieldPointsChangedSignal += OnShieldChanged;
	}

	private void SetProgressBar(ProgressBar progressBar, int maxValue, int? currentPoints = null, int minValue = 0, bool exp = false)
	{
		progressBar.Rounded = true;
		progressBar.MaxValue = maxValue;
		progressBar.Value = currentPoints ?? maxValue;
		progressBar.ShowPercentage = false;
		progressBar.MinValue = minValue;
		progressBar.ExpEdit = exp;
	}

	private void OnShieldChanged(int currentShieldPoints, bool isIncrease)
	{
		ShieldProgressBar.Value = currentShieldPoints;
		//Hide shield progress bar if shield points are 0
		ShieldProgressBar.Visible = currentShieldPoints != 0;
	}

	private void OnHealthChange(int currentHealth, bool isIncrease)
	{
		HealthProgressBar.Value = currentHealth;

		if ((float)HealthProgressBar.Value <= (float)HealthProgressBar.MaxValue * HealthBlinkThreshold)
		{
			double blinkSpeedUp = MaxBlinkSpeed * (1.0f - (HealthProgressBar.Value / HealthProgressBar.MaxValue));
			AnimationPlayer.Play(UiAnimations.OnHealthDanger, blinkSpeedUp);
		}
		else
		{
			AnimationPlayer.Play(UiAnimations.RESET);
		}
	}
}
