using AlfaEBetto.EnemyWeapons;
using Godot;

namespace AlfaEBetto.Enemies.Parts;

public sealed partial class TurretWing : Area2D
{
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
	[Export]
	public TurrentBase Turrent { get; set; }

	public void AllowShoot() => Turrent.AllowShoot();

	public void DisallowShoot() => Turrent.DisallowShoot();
}
