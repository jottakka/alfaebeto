using Godot;
public sealed partial class TextMeteor : Area2D
{
	[Export]
	public Label WordFirstPart { get; set; }
	[Export]
	public Label WordLastPart { get; set; }
	[Export]
	public Label QuestionMarkLabel { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Signal]
	public delegate void ReadyToQueueFreeSignalEventHandler();

	public override void _Ready()
	{
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public void Destroy(bool wasTargetDestroied)
	{
		if (wasTargetDestroied)
		{
			AnimationPlayer.Play(MeteorAnimations.TextMeteorDeathTargetHit);
		}
		else
		{
			AnimationPlayer.Play(MeteorAnimations.TextMeteorDeathTargetNotHit);
		}
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == MeteorAnimations.TextMeteorDeathTargetHit || animationName == MeteorAnimations.TextMeteorDeathTargetNotHit)
		{
			_ = EmitSignal(nameof(ReadyToQueueFreeSignal));
		}
	}
}
