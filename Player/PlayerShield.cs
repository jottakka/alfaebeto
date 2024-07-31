using Godot;

public sealed partial class PlayerShield : CharacterBody2D
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public HitBox HitBox { get; set; }
    [Export]
    public bool IsActive { get; set; } = true;

    private Player _player => GetParent<Player>();

    public override void _Ready()
    {
        MotionMode = MotionModeEnum.Floating;

        this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

        this.ActivateCollisionLayer(CollisionLayers.PlayerShield);

        this.ActivateCollisionMask(CollisionLayers.RegularEnemy);
        this.ActivateCollisionMask(CollisionLayers.WordEnemy);
        this.ActivateCollisionMask(CollisionLayers.MeteorEnemy);

        AnimationPlayer.Play(PlayerAnimations.RESET);

        AnimationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public void OnCollision()
    {
        AnimationPlayer.Play(PlayerAnimations.OnPlayerShieldHit);
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName == PlayerAnimations.OnPlayerShieldHit)
        {
            AnimationPlayer.Play(PlayerAnimations.RESET);
        }
    }
}
