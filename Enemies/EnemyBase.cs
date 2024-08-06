using Godot;
public partial class EnemyBase : CharacterBody2D
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public HitBox HitBox { get; set; }
    [Export]
    public EnemyHurtBox HurtBox { get; set; }
    [Export]
    public HurtComponent HurtComponent { get; set; }
    [Export]
    public HealthComponent HealthComponent { get; set; }
    [Export]
    public CoinSpawnerComponent CoinSpawnerComponent { get; set; }
    [Export]
    public Sprite2D SplatsSprite2D { get; set; }
    [Export]
    public float Speed { get; set; } = 60.0f;
    [Export]
    public float KnockBackFactor { get; set; } = 50.0f;

    public Vector2 InitialPosition { get; set; } = Vector2.Zero;
    public Vector2 SpawnInitialVelocity { get; set; } = Vector2.Zero;

    private Player _player => Global.Instance.Player;
    private bool _isSpawning = false;

    public override void _Ready()
    {
        // Randomize the splat sprite frame
        SplatsSprite2D.Frame = (int)(GD.Randi() % 5);

        this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

        DeactiveCollisions();

        GlobalPosition = InitialPosition;

        HurtComponent.OnHurtSignal += OnHurt;

        HealthComponent.OnHealthDepletedSignal += OnDeath;

        AnimationPlayer.AnimationFinished += OnAnimationFinished;

        if (_isSpawning)
        {
            SetAsSpawning();
        }
        else
        {
            OnReadyToAction();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (HealthComponent.IsDead)
        {
            return;
        }

        if (HurtComponent.IsHurt)
        {
            return;
        }

        if (_isSpawning)
        {
            Velocity += SpawnInitialVelocity;
        }
        else
        {
            Vector2 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
            Velocity = direction * Speed;
        }

        _ = MoveAndSlide();
    }

    public void SetAsSpawning()
    {
        DeactiveCollisions();
        _isSpawning = true;
        Visible = true;
        AnimationPlayer.Play(EnemyAnimations.EnemySpawn);
    }

    private void OnDeath()
    {
        DeactiveCollisions();
        AnimationPlayer.Play(EnemyAnimations.EnemyBugDie);
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName == EnemyAnimations.EnemyBugDie)
        {
            CoinSpawnerComponent.SpawnCoins(GlobalPosition);
            QueueFree();
        }

        if (animationName == EnemyAnimations.EnemyBugHurtBlink)
        {
            OnHurtStateFinished();
        }

        if (animationName == EnemyAnimations.EnemySpawn)
        {
            _isSpawning = false;

            OnReadyToAction();
        }
    }

    private void OnHurtStateFinished()
    {
        AnimationPlayer.Play(EnemyAnimations.EnemyBugMoving);
        HurtComponent.OnHurtStateFinished();
        ActivateCollisions();
    }

    private void OnReadyToAction()
    {
        AnimationPlayer.Play(EnemyAnimations.EnemyBugMoving);
        ActivateCollisions();
    }

    private void OnHurt(Area2D enemyArea)
    {
        if (_isSpawning is false)
        {
            DeactiveCollisions();
            if (enemyArea is PlayerSpecialHurtBox)
            {
                HealthComponent.TakeDamage(10);
            }
            else if (enemyArea.GetParent() is OwlFriend owlFriend)
            {
                HealthComponent.TakeDamage(owlFriend.HitPoints);

            }

            if (HealthComponent.IsDead is false)
            {
                AnimationPlayer.Play(EnemyAnimations.EnemyBugHurtBlink);
            }

            Vector2 knockVelocity = GlobalPosition.DirectionTo(enemyArea.GlobalPosition);
            Position -= knockVelocity * KnockBackFactor;
        }
    }

    private void ActivateCollisions()
    {
        HurtBox.ActivateCollisionsMasks();
        HitBox.ActivateCollisionsMasks();
    }

    private void DeactiveCollisions()
    {
        this.ResetCollisionLayerAndMask();
        HurtBox.DeactivateCollisions();
        HitBox.DeactivateCollisions();
    }
}

