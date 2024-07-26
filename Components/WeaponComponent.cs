using Godot;


public sealed partial class WeaponComponent : Node
{
	[Export]
	public PackedScene LaserPackedScene { get; set; }
	[Export]
	public PlayerInputProcessor PlayerInputProcessor { get; set; }
	[Export]
	public Timer CooldownTimer { get; set; }
	[Export]
	public Player Player { get; set; }

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
		var laser = LaserPackedScene.Instantiate<Laser>();
		laser.Position = Player.MuzzlePosition.GlobalPosition;
		Player.GetParent().AddChild(laser);
		CooldownTimer.Start();
	}

	private void OnCooldownTimeout()
	{
		_isWaitingCooldown = false;
	}
}

