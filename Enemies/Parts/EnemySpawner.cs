using Godot;

public sealed partial class EnemySpawner : Area2D
{
    [Export]
    public EnemySpawnerControllerComponent SpawnerController { get; set; }
    [Export]
    public Marker2D Muzzle { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Signal]
    public delegate void OnSpawnEnemyReadySignalEventHandler();
    [Signal]
    public delegate void OnSpawnProcessingFinishedSignalEventHandler();

    public override void _Ready()
    {
        AnimationPlayer.AnimationFinished += (StringName animationName) =>
        {
            if (animationName == EnemyPartAnimations.SpawnEnemy)
            {
                //AnimationPlayer.Play(EnemyPartAnimations.SpawnerMoving);
                EmitSignal(nameof(OnSpawnProcessingFinishedSignal));
            }
        };
    }

    public void StartSpawn()
    {
        AnimationPlayer.Play(EnemyPartAnimations.SpawnEnemy);
    }

    public void OnSpawnEnemyAnimationPointReady()
    {
        EmitSignal(nameof(OnSpawnEnemyReadySignal));
    }
}

