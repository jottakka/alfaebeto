using Godot;

public sealed partial class MainNode : Node2D
{
	[Export]
	public PackedScene StartGamePackedScene { get; set; }
	[Export]
	public PackedScene StorePackedScene { get; set; }
	[Export]
	public PackedScene RulesPackedScene { get; set; }

	public override void _Ready()
	{

	}
}
