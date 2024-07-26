using Godot;
using System.Linq;

public sealed partial class Laser : Area2D
{
	[Export]
	public Sprite2D Sprite2D { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public PlayerSpecialHurtBox PlayerSpecialHurtBox { get; set; }
	[Export]
	public HitBox HitBox { get; set; }
	[Export]
	public static float CooldownSecs { get; set; } = 0.1f;
	[Export]
	public float Speed { get; set; } = 900.0f;

	public override void _Ready()
	{
		this.SetVisibilityZOrdering(VisibilityZOrdering.Ammo);
		VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
		AnimationPlayer.AnimationFinished += (StringName animationName) =>
		{
			if (animationName == WeaponAnimations.LaserOnHit)
			{
				QueueFree();
			}
		};

	}

	public override void _PhysicsProcess(double delta)
	{
		var overlapingArea = HitBox.GetOverlappingAreas().FirstOrDefault();

		if (overlapingArea is not null)
		{
			AnimationPlayer.Play(WeaponAnimations.LaserOnHit);
			Speed = 0;
		}
		Position += new Vector2(0, -(Speed * (float)delta));
	}

	private void OnScreenExited()
	{
		QueueFree();
	}
}
