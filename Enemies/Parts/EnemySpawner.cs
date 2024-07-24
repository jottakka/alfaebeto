using Godot;

public sealed partial class EnemySpawner : Area2D
{
    [Export]
    public EnemySpawnerControllerComponent SpawnerController { get; set; }
}

