using Godot;
public sealed partial class TurrentBase : Area2D
{
	[Export]
	public TurrentControllerComponent TurrentControllerComponent { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public Marker2D Muzzle { get; set; }
	[Export]
	public Timer CooldownTimer { get; set; }
	[Export]
	public float RotationSpeed { get; set; } = Mathf.Pi / 10.0f;
	[Signal]
	public delegate void ShootPointReachedSignalEventHandler();

	private bool _isAllowedToShoot = false;
	private bool _isCooldownTimoutFinished = true;

	public override void _Ready()
	{
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
		CooldownTimer.Timeout += CooldownTimerTimeout;
	}

	public void AllowShoot()
	{
		_isAllowedToShoot = true;

	}

	public void DesallowShoot()
	{
		_isAllowedToShoot = false;
	}

	public void Shoot()
	{
		if (_isAllowedToShoot)
		{
			AnimationPlayer.Play(EnemyWeaponAnimations.TurrentShoot);
		}
	}

	public void OnAnimationShootReady()
	{
		EmitSignal(nameof(ShootPointReachedSignal));
	}

	private void CooldownTimerTimeout()
	{
		_isCooldownTimoutFinished = true;
	}

	private void OnAnimationFinished(StringName animationFinished)
	{
		if (animationFinished == EnemyWeaponAnimations.TurrentShoot)
		{
			CooldownTimer.Start();
			_isCooldownTimoutFinished = false;
		}
	}
}
