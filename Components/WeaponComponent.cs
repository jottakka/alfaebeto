using Godot;

public sealed partial class WeaponComponent : Node
{
	[Export]
	public PackedScene LaserPackedScene { get; set; }
	[Export]
	public PlayerInputProcessor PlayerInputProcessor { get; set; }
	[Export]
	public Timer CooldownTimer { get; set; }

	private Player _player => Global.Instance.Player;

	private StageBase _scene => Global.Instance.Scene;

	private bool _isWaitingCooldown = false;

	public override void _Ready()
	{
		CooldownTimer.Timeout += OnCooldownTimeout;
	}

	public override void _Process(double delta)
	{
		if (PlayerInputProcessor.IsAttacking && !_isWaitingCooldown)
		{
			ShootLaser();
		}
	}

	private void ShootLaser()
	{
		_isWaitingCooldown = true;
		Laser laser = LaserPackedScene.Instantiate<Laser>();
		laser.Position = _player.MuzzlePosition.GlobalPosition;
		_scene.AddChildDeffered(laser);
		CooldownTimer.Start();
	}

	private void OnCooldownTimeout()
	{
		_isWaitingCooldown = false;
	}
}

