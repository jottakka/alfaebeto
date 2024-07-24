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
	public float Speed { get; set; } = 100.0f;
	[Export]
	public float KnockBackFactor { get; set; } = 50.0f;
	private Player _player => Global.Instance.Player;
	public override void _Ready()
	{
		this.SetVisibilityZOrdering(VisibilityZOrdering.PlayerAndEnemies);

		HurtComponent.OnHurtSignal += (Area2D enemyArea) =>
		{
			AnimationPlayer.Play(EnemyAnimations.EnemyBugHurtBlink);
			var knockVelocity = GlobalPosition.DirectionTo(enemyArea.GlobalPosition);
			Position -= knockVelocity * KnockBackFactor;
		};

		AnimationPlayer.AnimationFinished += (StringName animationName) =>
		{
			if (animationName == EnemyAnimations.EnemyBugHurtBlink)
			{
				AnimationPlayer.Play(EnemyAnimations.EnemyBugMoving);
				HurtComponent.OnHurtStateFinished();
			}
		};

		// Collidion layer to act upon
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemy);
		this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHurtBox);

		AnimationPlayer.Play(EnemyAnimations.EnemyBugMoving);
	}

	public override void _PhysicsProcess(double delta)
	{
		if(HurtComponent.IsHurt is false)
		{
			var direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
			Position += direction * Speed * (float)delta;
			MoveAndSlide();
		}
	}
}

