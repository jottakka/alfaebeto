using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    [Signal]
    public delegate void OnSpawnNextRequestedSignalEventHandler();
    private Node _parent => GetParent();

 
    public void SpawnNextSpecial()
    {
        
    }
}
