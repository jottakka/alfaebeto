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
    [Export]
    public SceneEnemySpawnerComponent SceneEnemySpawnerComponent { get; set; }

    public override void _Ready()
    {
        Global.Instance.Player = Player;
        Global.Instance.Scene = this;
        Player.GlobalPosition = PlayerSpawnPosition.Position;
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
}
