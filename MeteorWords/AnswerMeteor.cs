using Godot;

public sealed partial class AnswerMeteor : StaticBody2D
{
    [Export]
    public HitBox HitBox { get; set; }
    [Export]
    public HurtComponent HurtComponent { get; set; }
    [Export]
    public EnemyHurtBox HurtBox { get; set; }
    [Export]
    public HealthComponent HealthComponent { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public AnimationPlayer EffectsPlayer { get; set; }
    [Export]
    public Label OptionText { get; set; }

    [Signal]
    public delegate void OnTargetDestroiedSignalEventHandler();

    public bool IsTarget { get; set; } = false;

    public override void _Ready()
    {
        HurtComponent.OnHurtSignal += OnHurt;
        HealthComponent.OnHealthDepletedSignal += OnHealthDepleted;
        AnimationPlayer.Play(MeteorAnimations.AnswerMeteorMoving);

        ActivateBodyCollisions();
    }

    public void DeactivateCollisions()
    {
        HitBox.DeactivateCollisionMasks();
        HurtBox.DeactivateCollisionMasks();
        DeactivateBodyCollisions();
    }

    public void ActivateCollisions()
    {
        HitBox.ActivateCollisionsMasks();
        HurtBox.ActivateCollisionsMasks();
        ActivateBodyCollisions();
    }

    private void DeactivateBodyCollisions()
    {
        this.ResetCollisionLayerAndMask();
    }

    private void ActivateBodyCollisions()
    {
        this.ActivateCollisionLayer(CollisionLayers.MeteorEnemy);
    }

    private void OnHealthDepleted()
    {
        if (IsTarget)
        {
            EmitSignal(nameof(OnTargetDestroiedSignal));
        }
    }

    private void OnHurt(Area2D enemyArea)
    {
        HealthComponent.TakeDamage(10);
        EffectsPlayer.Play(MeteorAnimations.AnswerMeteorHurt);
    }
}

