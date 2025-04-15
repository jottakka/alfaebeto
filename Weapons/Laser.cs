using System.Linq;
using Godot;

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
	public float CooldownSecs { get; set; } = 0.1f;
	[Export]
	public float Speed { get; set; } = 900.0f;
	[Export]
	public float LaserRange { get; set; } = 500.0f;

	private float _distance = 0.0f;

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
		Area2D overlapingArea = HitBox.GetOverlappingAreas().FirstOrDefault();

		if (overlapingArea is not null)
		{
			AnimationPlayer.Play(WeaponAnimations.LaserOnHit);
			Speed = 0;
		}

		if (_distance >= LaserRange)
		{
			QueueFree();
		}

		_distance += Speed * (float)delta;
		Position += new Vector2(0, -(Speed * (float)delta));
	}

	private void OnScreenExited() => QueueFree();
}
