using System;
using Godot;

public sealed partial class SceneEnemySpawnerComponent : Node
{
    [Export]
    public Marker2D SpecialSpawnerPosition { get; set; }
    [Export]
    public PackedScene[] SpecialEnemiesPackedScenes { get; set; } = Array.Empty<PackedScene>();
    [Export]
    public PackedScene[] RegularEnemiesPackedScenes { get; set; } = Array.Empty<PackedScene>();
    [Export]
    public PackedScene[] WordMeteorPackedScenes { get; set; } = Array.Empty<PackedScene>();
    [Export]
    public PackedScene[] MeteorPackedScenes { get; set; } = Array.Empty<PackedScene>();
    [Export]
    public PathFollow2D SpawnFollowPath { get; set; }
    [Export]
    public Timer MeteorSpawnTimer { get; set; }
    [Export]
    public Timer MeteorWordSpawnTimer { get; set; }
    [Export]
    public Timer SpecialEnemySpawnTimer { get; set; }

    [Signal]
    public delegate void OnSpawnNextRequestedSignalEventHandler();

    private States _currentState = States.NoSpecialEnemy;
    private Node _parent => GetParent();

    public override void _Ready()
    {
        SpawnFollowPath.Rotates = false;
        SpawnFollowPath.Loop = false;
        SpawnNextSpecial();
        SpawnEnemy(MeteorPackedScenes);
        SpawnEnemy(MeteorPackedScenes);

        MeteorSpawnTimer.Timeout += () => SpawnEnemy(MeteorPackedScenes);
        MeteorWordSpawnTimer.Timeout += SpawnNextWordMeteor;
        SpecialEnemySpawnTimer.Timeout += SpawnNextSpecial;
        MeteorWordSpawnTimer.Start();
        SpecialEnemySpawnTimer.Start();
        MeteorSpawnTimer.Start();
    }

    private void SpawnEnemy(PackedScene[] packedScenes)
    {
        long idx = GD.Randi() % packedScenes.Length;
        Node2D enemySceneInstantiated = packedScenes[idx].Instantiate<Node2D>();
        enemySceneInstantiated.Position = GetNewTopSpawnPathRandPosition();
        _parent.AddChildDeffered(enemySceneInstantiated);
    }

    public void SpawnNextSpecial()
    {
        SpecialEnemySpawnTimer.Stop();
        _currentState = States.SpecialEnemyAlive;

        EnemyWord enemySceneInstantiated = SpecialEnemiesPackedScenes[0].Instantiate<EnemyWord>();
        enemySceneInstantiated.Position = SpecialSpawnerPosition.Position;
        enemySceneInstantiated.OnQueueFreeSignal += OnSpecialEnemyFreed;

        _parent.AddChildDeffered(enemySceneInstantiated);
    }

    public void SpawnNextWordMeteor()
    {
        if (_currentState is States.SpecialEnemyAlive)
        {
            return;
        }

        MeteorWordTarget enemySceneInstantiated = WordMeteorPackedScenes[0].Instantiate<MeteorWordTarget>();
        Vector2 randonHorizontalVariation = new((float)GD.RandRange(-75.0f, 75.0f), 0.0f);
        enemySceneInstantiated.Position = SpecialSpawnerPosition.Position + randonHorizontalVariation;
        _parent.AddChildDeffered(enemySceneInstantiated);
    }

    private void OnSpecialEnemyFreed()
    {
        _currentState = States.NoSpecialEnemy;
        SpecialEnemySpawnTimer.Start();
    }

    private Vector2 GetNewTopSpawnPathRandPosition()
    {
        SpawnFollowPath.ProgressRatio = GD.Randf();
        return SpawnFollowPath.GlobalPosition;
    }

    private enum States
    {
        SpecialEnemyAlive,
        NoSpecialEnemy,
    }
}
