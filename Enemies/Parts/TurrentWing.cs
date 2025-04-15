using Godot;

public sealed partial class TurrentWing : Area2D
{
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
	[Export]
	public TurrentBase Turrent { get; set; }

	public void AllowShoot() => Turrent.AllowShoot();

	public void DesallowShoot() => Turrent.DesallowShoot();
}

