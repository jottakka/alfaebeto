using Godot;
using System;

public sealed class EnemyBuilder
{
    public EnemyBuilder(PackedScene enemyPackedScene)
    {
        _enemyPackedScene = enemyPackedScene;
    }

    private PackedScene _enemyPackedScene;

    public EnemyBase Create(
        Vector2 position,
        Vector2 velocity
    )
    {
        var enemy = _enemyPackedScene.Instantiate<EnemyBase>();
        enemy.Velocity = velocity;
        enemy.InitialPosition = position;
        return enemy;
    }
}

