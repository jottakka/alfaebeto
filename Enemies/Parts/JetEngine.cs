using Godot;

public sealed partial class JetEngine : Area2D
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }

	public override void _Ready()
	{
		AnimationPlayer.Play(EnemyPartAnimations.JetEngineMoving);
	}
}
