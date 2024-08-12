using AlfaEBetto.UI;
using Godot;

public sealed partial class StartGame : Node2D
{
	[Export]
	public Player Player { get; set; }
	[Export]
	public StageBase Stage { get; set; }
	[Export]
	public GameOverUi GameOverUi { get; set; }
	[Export]
	public PauseMenuUi PauseMenuUi { get; set; }

	public override void _Ready()
	{
		Global.Instance.SettingMainNodeData(Player, Stage);

		Player.OnPlayerDeathSignal += OnPlayerDeath;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionPressed(UserInput.Pause))
		{
			PauseMenuUi.Pause();
		}
	}

	private void OnPlayerDeath()
	{
		Show();
		GetTree().Paused = true;
		GameOverUi.Open();
	}
}
