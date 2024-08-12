using Godot;

namespace AlfaEBetto.UI;
public sealed partial class GameOverUi : Control
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public Button ProcceedButton { get; set; }
	[Export]
	public SceneManagerComponent SceneManagerComponent { get; set; }

	public override void _Ready()
	{
		Hide();
		ProcessMode = ProcessModeEnum.Always;
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
		ProcceedButton.Pressed += () =>
		{
			SceneManagerComponent.SwitchToMainMenu();
		};
	}

	private void OnMainNodeReady()
	{
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == UiAnimations.OnGameOverStart)
		{
			AnimationPlayer.Play(UiAnimations.OnGameOverLoop);
		}
	}

	public void Open()
	{
		AnimationPlayer.Play(UiAnimations.OnGameOverStart);
	}
}
