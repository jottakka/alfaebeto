using Godot;

public sealed partial class AnswerMeteor : Area2D
{
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public HurtComponent HurtComponent { get; set; }
	[Export]
	public HealthComponent HealthComponent { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public AnimationPlayer EffectsPlayer { get; set; }
	[Export]
	public Label OptionText { get; set; }

	public override void _Ready()
	{
		HurtComponent.OnHurtSignal += OnHurt;
		AnimationPlayer.Play(MeteorAnimations.AnswerMeteorMoving);
	}

	//private void OnHurtAnimationFinished(StringName animationName)
	//{
	//    if (animationName == LetterBlockAnimations.Hurt)
	//    {
	//        AnimationPlayer.Play(LetterBlockAnimations.RESET);
	//    }
	//}

	private void OnHurt(Area2D enemyArea)
	{
		HealthComponent.TakeDamage(10);
		EffectsPlayer.Play(MeteorAnimations.AnswerMeteorHurt);
	}
}

