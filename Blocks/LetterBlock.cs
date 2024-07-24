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
			}
		};
		//// Collidion layer to act upon
		//this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);
		//this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		//this.ActivateCollisionLayer(CollisionLayers.WordEnemy);

		//// Collision Masks to observe
		//this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
