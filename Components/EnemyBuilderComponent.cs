using AlfaEBetto.Enemies;
using Godot;

namespace AlfaEBetto.Components;

public sealed class EnemyBuilder
{
	public EnemyBuilder(PackedScene enemyPackedScene) => _enemyPackedScene = enemyPackedScene;

	private readonly PackedScene _enemyPackedScene;

	public EnemyBase Create(
		Vector2 position,
		Vector2 velocity
	)
	{
		EnemyBase enemy = _enemyPackedScene.Instantiate<EnemyBase>();
		enemy.Velocity = velocity;
		enemy.InitialPosition = position;
		return enemy;
	}
}
