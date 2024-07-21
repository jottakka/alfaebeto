using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public sealed partial class Player :  CharacterBody2D
{
	[Export]
	public Sprite2D Sprite2D { get; set; }
	[Export]
	public CollisionPolygon2D CollisionPolygon2D { get; set; }
	[Export]
	public PlayerInputProcessor PlayerInputProcessor { get; set; }
	[Export]
	public Marker2D MuzzlePosition { get; set; }
	[Export]
	public WeaponComponent WeaponComponent { get; set; }
	[Export]
	public float Speed { get; set; } = 400.0f;

	public override void _Ready()
	{
		ZIndex = (int)VisibilityZOrdering.PlayerAndEnemies;
	}

	public override void _PhysicsProcess(double delta)
	{
		var movementDirection = PlayerInputProcessor.MovementDirection;
		Velocity = movementDirection * (Speed * (float)delta);
		MoveAndCollide(Velocity);
		GlobalPosition= new Vector2(
			Mathf.Clamp(GlobalPosition.X, 0, GetViewportRect().Size.X),
			Mathf.Clamp(GlobalPosition.Y, 0, GetViewportRect().Size.Y)
		);
	}   
}
