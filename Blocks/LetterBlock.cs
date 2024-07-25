using Godot;
using System;

public partial class LetterBlock : StaticBody2D
{
	[Export]
	public Sprite2D Sprite { get; set; }
	[Export]
	public Label Label { get; set; }
	[Export]
	public CollisionShape2D CollisionShape { get; set; }
	[Export]
	AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public HurtComponent HurtComponent { get; set; }
	[Export]
	public bool IsTarget { get; set; }

	[Signal]
	public delegate void OnTargetDestructedSignalEventHandler();

	public override void _Ready()
	{
		this.ResetCollisionLanyerAndMask();
		HurtComponent.OnHurtSignal += (Area2D enemyArea) =>
		{
			AnimationPlayer.Play(LetterBlockAnimations.Hurt);
		};

		AnimationPlayer.AnimationFinished += (StringName animationName) =>
		{
			if (animationName == LetterBlockAnimations.Hurt)
			{
				AnimationPlayer.Play(LetterBlockAnimations.RESET);

				if(IsTarget)
				{
					EmitSignal(nameof(OnTargetDestructedSignal));
				}
			}
		};
	}

	public void SetLabel(char letter)
	{
		Label.Text = letter.ToString();
	}

	public void SetPosition(Vector2 position)
	{
		Position = position;
	}
}
