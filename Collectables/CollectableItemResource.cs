using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[GlobalClass]
public sealed partial class CollectableItemResource : Resource
{
    [Export]
    public string Name { get; set; } = "";
    [Export]
    public PackedScene Scene { get; set; }
    [Export]
    public Texture2D Texture { get; set; }
    [Export]
    public string AnimationOnCollect { get; set; }
}

