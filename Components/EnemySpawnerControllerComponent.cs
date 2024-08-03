using Godot;

public sealed partial class EnemySpawnerControllerComponent : Node
{
	[Export]
	public PackedScene EnemyPackedScene { get; set; }
	[Export]
	public Timer CooldownTimer { get; set; }
	[Export]
	public float BaseCooldown { get; set; } = 1.5f;
	[Export]
	public float CooldownVariance { get; set; } = 0.2f;
	[Export]
	public float SpawnSpeed { get; set; } = 100.0f;

	private EnemyBuilder _enemyBuilder;
	private Node _scene => Global.Instance.Scene;
	private EnemySpawner _enemySpawner;
	private bool _allowToShoot = false;

	public override void _Ready()
	{
		_enemySpawner = GetParent<EnemySpawner>();
		_enemyBuilder = new EnemyBuilder(EnemyPackedScene);

		CooldownTimer.WaitTime = GetRandomCooldownTime();
		CooldownTimer.Timeout += _enemySpawner.StartSpawn;

		_enemySpawner.OnSpawnEnemyReadySignal += SpawnProjectile;
		_enemySpawner.OnSpawnProcessingFinishedSignal += OnReadyToRestarTimer;
		_enemySpawner.OnSpawnerPermissionChangeSignal += OnSpawnerPermissionChange;
	}

	private void OnSpawnerPermissionChange(bool isAllowedToShoot)
	{
		_allowToShoot = isAllowedToShoot;
		if (_allowToShoot)
		{
			CooldownTimer.Start();
		}
		else
		{
			CooldownTimer.Stop();
		}
	}

	private void OnReadyToRestarTimer()
	{
		CooldownTimer.WaitTime = GetRandomCooldownTime();
		CooldownTimer.Start();
	}

	private double GetRandomCooldownTime()
	{
		return GD.Randfn(BaseCooldown, CooldownVariance);
	}

	private void SpawnProjectile()
	{
		Vector2 velocity = _enemySpawner.GlobalPosition.DirectionTo(_enemySpawner.Muzzle.GlobalPosition) * SpawnSpeed;
		EnemyBase enemy = _enemyBuilder.Create(
			_enemySpawner.Muzzle.GlobalPosition,
			velocity
		);
		enemy.SetAsSpawning();
		_scene.AddChildDeffered(enemy);
	}
}
