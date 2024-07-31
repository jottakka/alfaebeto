using Godot;
using WordProcessing.Models.XorCH;

public sealed partial class MeteorWordTarget : Area2D
{
    [Export]
    public TextMeteor MainMeteor { get; set; }
    [Export]
    public AnswerMeteor AnswerMeteor1 { get; set; }
    [Export]
    public AnswerMeteor AnswerMeteor2 { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; set; }
    [Export]
    public float Speed { get; set; } = 50.0f;

    private XorCHWord _xorCHWord;

    private AnswerMeteor _targetMeteor;

    public override void _Ready()
    {
        _xorCHWord = Global.Instance.XorChWords.Dequeue();

        VisibleOnScreenNotifier2D.ScreenExited += QueueFree;

        BuildAnserMeteors();

        _targetMeteor.OnTargetDestroiedSignal += QueueFree;

        MainMeteor.WordFirstPart.Text = _xorCHWord.FirstPart;
        MainMeteor.WordLastPart.Text = _xorCHWord.SecondPart;

        AnimationPlayer.Play(MeteorAnimations.MeteorWordOrbiting);
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += new Vector2(0, Speed * (float)delta);
    }

    private void BuildAnserMeteors()
    {
        AnswerMeteor1.OptionText.Text = "ch";
        AnswerMeteor2.OptionText.Text = "x";
        _targetMeteor = _xorCHWord.RightOption == "ch"
            ? AnswerMeteor1
            : AnswerMeteor2;
        _targetMeteor.IsTarget = true;

    }
}

