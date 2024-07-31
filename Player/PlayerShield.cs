using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public sealed partial class PlayerShield : CharacterBody2D
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public bool IsActive { get; set; } = true;

	private Player _player => this.GetParent<Player>();

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

	private void OnCollision(Node node)
	{
		AnimationPlayer.Play(PlayerAnimations.OnPlayerShieldHit);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if(animationName == PlayerAnimations.OnPlayerShieldHit)
		{
			AnimationPlayer.Play(PlayerAnimations.RESET);
		}
	}
}
