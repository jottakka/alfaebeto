using Godot;

public sealed partial class FullUi : Control
{
	[Export]
	public MoneyCounterUi MoneyCounterUi { get; set; }
	[Export]
	public HeartShieldUi HeartShieldUi { get; set; }
}
