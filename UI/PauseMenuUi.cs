using Godot;
public sealed partial class PauseMenuUi : Control
{
	[Export]
	public UiComponent UiComponent { get; set; }
	[Export]
	public Button ContinueButton { get; set; }
	[Export]
	public Button ExitButton { get; set; }
	[Export]
	public Button RulesButton { get; set; }

	private Global _global => Global.Instance;

	public override void _Ready()
	{
		Hide();
		this.SetVisibilityZOrdering(VisibilityZOrdering.UI);
		ProcessMode = ProcessModeEnum.Always;
		Global.Instance.OnMainNodeSetupFinishedSignal += OnMainNodeReady;
	}

	public void Pause()
	{
		GetTree().Paused = true;
		Show();
	}

	private void OnMainNodeReady()
	{
		RulesButton.Pressed += () =>
		{
			UiComponent.OpenRuleSetsViewingUi();
		};
		ContinueButton.Pressed += () =>
		{
			GetTree().Paused = false;
			Hide();
		};
		ExitButton.Pressed += () =>
		{
			GetTree().Paused = false;

			_global.SwitchToMainMenu();
		};
	}
}
