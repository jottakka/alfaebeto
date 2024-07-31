using Godot;
public partial class EnemyBase : CharacterBody2D
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public HitBox HitBox { get; set; }
    [Export]
    public HurtComponent HurtComponent { get; set; }
    [Export]
    public HealthComponent HealthComponent { get; set; }
    [Export]
    public CoinSpawnerComponent CoinSpawnerComponent { get; set; }
    [Export]
    public float Speed { get; set; } = 100.0f;
    [Export]
    public float KnockBackFactor { get; set; } = 50.0f;

    public Vector2 InitialPosition { get; set; } = Vector2.Zero;
    public Vector2 SpawnInitialVelocity { get; set; } = Vector2.Zero;

    private Player _player => Global.Instance.Player;
    private bool _isSpawning = false;

    public override void _Ready()
    {
        Visible = false;
        this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);
        GlobalPosition = InitialPosition;

        HurtComponent.OnHurtSignal += OnHurt;

        AnimationPlayer.AnimationFinished += OnAnimationFinished;

        HealthComponent.OnHealthDepletedSignal += OnDeath;

        // Collidion layer to act upon
        this.ActivateCollisionLayer(CollisionLayers.RegularEnemy);
        this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHurtBox);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (HurtComponent.IsHurt)
        {
            return;
        }
        if (_isSpawning)
        {
            Position += SpawnInitialVelocity * (float)delta;
        }
        else
        {
            var direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * Speed * (float)delta;
            Position += direction * Speed * (float)delta;
        }
        MoveAndSlide();
    }

    public void SetAsSpawning()
    {
        _isSpawning = true;
        AnimationPlayer.Play(EnemyAnimations.EnemySpawn);
        Visible = true;
    }

    private void OnDeath()
    {
        CoinSpawnerComponent.SpawnCoins(GlobalPosition);
        QueueFree();
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName == EnemyAnimations.EnemyBugHurtBlink)
        {
            OnHurtStateFinished();
        }
        if (animationName == EnemyAnimations.EnemySpawn)
        {
            OnSpawnStateFinished();
        }
    }

    private void OnHurtStateFinished()
    {
        AnimationPlayer.Play(EnemyAnimations.EnemyBugMoving);
        HurtComponent.OnHurtStateFinished();
    }

    private void OnSpawnStateFinished()
    {
        HitBox.ActivateCollisionsMasks();
        AnimationPlayer.Play(EnemyAnimations.EnemyBugMoving);
        _isSpawning = false;
    }

    private void OnHurt(Area2D enemyArea)
    {
        if (_isSpawning is false)
        {
            if (enemyArea is PlayerSpecialHurtBox)
            {
                HealthComponent.TakeDamage(10);
            }
            AnimationPlayer.Play(EnemyAnimations.EnemyBugHurtBlink);
            var knockVelocity = GlobalPosition.DirectionTo(enemyArea.GlobalPosition);
            Position -= knockVelocity * KnockBackFactor;
        }
    }
}

