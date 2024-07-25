using Godot;

public sealed partial class MeteorWordTarget : Area2D
{
    [Export]
    public AnswerMeteor AnswerMeteor1 { get; set; }
    [Export]
    public AnswerMeteor AnswerMeteor2 { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
    [Export]
    public float Speed { get; set; } = 30.0f;

    public override void _Ready()
    {
        AnimationPlayer.Play(MeteorAnimations.MeteorWordOrbiting);

        VisibleOnScreenNotifier2D.ScreenExited += QueueFree;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += new Vector2(0, Speed * (float)delta);
    }
}

