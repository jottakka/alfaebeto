using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public sealed partial class Laser : Area2D
{
	[Export]
	public Sprite2D Sprite2D { get; set; }
	[Export]
	public CollisionShape2D CollisionShape2D { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
	[Export]
	public static float CooldownSecs { get; set; } = 0.1f;
	[Export]
	public float Speed { get; set; } = 900.0f;

	public override void _Ready()
	{
		VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
		ZIndex = (int)VisibilityZOrdering.Ammo;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += new Vector2(0, -(Speed * (float)delta));
	}

	private void OnScreenExited()
	{
		QueueFree();
	}
}
