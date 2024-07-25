using Godot;

public partial class AmmoBase : Area2D
{
    [Export]
    public Sprite2D Sprite { get; set; }
    [Export]
    public CollisionShape2D CollisionShape { get; set; }
    [Export]
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier { get; set; }
    [Export]
    public float ShootRadAngle { get; set; }
    [Export]
    public Vector2 InitialPosition { get; set; }
    [Export]
    public float Speed { get; set; } = 600.0f;

    private Vector2 _direction;

    public override void _Ready()
    {
        VisibleOnScreenNotifier.ScreenExited += OnScreenExited;
        Rotate(ShootRadAngle);
        _direction = new Vector2(1, 0).Rotated(ShootRadAngle);

        GlobalPosition = InitialPosition;
        ZIndex = (int)VisibilityZOrdering.Ammo;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += _direction * Speed * (float)delta;
    }

    private void OnScreenExited()
    {
        QueueFree();
    }
}

