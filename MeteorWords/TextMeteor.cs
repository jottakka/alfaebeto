using Godot;
public sealed partial class TextMeteor : Area2D
{
    [Export]
    public Label Word { get; set; }
    [Export]
    public Label QuestionMarkLabel { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void _Ready()
    {
        AnimationPlayer.Play(MeteorAnimations.WordMeteorHurt);
    }
}
