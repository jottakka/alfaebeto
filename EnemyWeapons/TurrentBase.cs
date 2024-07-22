using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
public sealed partial class TurrentBase : Area2D
{
	[Export]
	public float RotationSpeed { get; set; } = Mathf.Pi / 6.0f;
	[Export]
	public float ShootCooldown { get; set; } = 0.3f;
	[Export]
	public TurrentControllerComponent TurrentControllerComponent { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public Marker2D Muzzle { get; set; }
	[Signal]
	public delegate void ShootPointReachedSignalEventHandler();

	public void Shoot()
	{
		AnimationPlayer.Play("shoot");
	}

	public void OnAnimationShootReady()
	{
		GD.Print("Shoot!");
		EmitSignal(nameof(ShootPointReachedSignal));
	}
}
