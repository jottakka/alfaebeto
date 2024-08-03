using Godot;

public sealed partial class MainNode : Node2D
{
	[Export]
	public Player Player { get; set; }
	[Export]
	public StageBase Stage { get; set; }

	public override void _Ready()
	{
		Global.Instance.SettingMainNodeData(Player, Stage);
	}
}
