using Godot;

namespace AlfaEBetto.UI;
public sealed partial class GameOverUi : Control
{
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    private Player _player => Global.Instance.Player;

    public override void _Ready()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;
        this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
        Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
    }

    private void OnMainNodeReady()
    {
        _player.OnPlayerDeathSignal += OnPlayerDeath;
        AnimationPlayer.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (animationName == UiAnimations.OnGameOverStart)
        {
            AnimationPlayer.Play(UiAnimations.OnGameOverLoop);
        }
    }

    private void OnPlayerDeath()
    {
        Visible = true;
        GetTree().Paused = true;
        AnimationPlayer.Play(UiAnimations.OnGameOverStart);
    }
}
