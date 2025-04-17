using Godot;

namespace AlfaEBetto.UI
{
	public sealed partial class FullUi : Control
	{
		[Export]
		public MoneyCounterUi MoneyCounterUi { get; set; }
		[Export]
		public HeartShieldUi HeartShieldUi { get; set; }
		[Export]
		public GemsUi GemsUi { get; set; }
	}
}
