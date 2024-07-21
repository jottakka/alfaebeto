using Godot;

public sealed partial class MainNode : Node2D
{
	[Export]
	public WordBuilderComponent WordBuilderComponent;
	[Export]
	public Player Player { get; set; }
	[Export]
	public Marker2D PlayerSpawnPosition { get; set; }
	[Export]
	public ParallaxBackground ParallaxBackground { get; set; }
	public override void _Ready()
	{
		Player.GlobalPosition = PlayerSpawnPosition.Position;
		var word = WordBuilderComponent.BuildWord("TESTE", new Vector2(0, 0));
		AddChild(word);
	}
	public override void _Process(double delta)
	{
		UpdateParallaxOffset(delta);

	}

	private void UpdateParallaxOffset(double delta)
	{
		ParallaxBackground.ScrollOffset += new Vector2(0, 100.0f * (float)delta);
		ParallaxBackground.ScrollOffset %= GetViewportRect().Size.Y;
	}

	public override void _PhysicsProcess(double delta)
	{
	}
}
