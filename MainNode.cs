using Godot;

public sealed partial class MainNode : Node2D
{
	[Export]
	public WordBuilderComponent WordBuilderComponent;
	public override void _Ready()
	{
		var word = WordBuilderComponent.BuildWord("TESTE", new Vector2(0, 0));
		AddChild(word);
	}
}
