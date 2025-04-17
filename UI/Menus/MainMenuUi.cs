using AlfaEBetto.Components;
using Godot;

namespace AlfaEBetto.Data.Words
{
	public sealed partial class MainMenuUi : Node
	{
		[Export]
		public Button StartButton { get; set; }
		[Export]
		public Button StoreButton { get; set; }
		[Export]
		public Button RulesButton { get; set; }
		[Export]
		public UiComponent UiComponent { get; set; }

		private Global _global => Global.Instance;

		public override void _Ready()
		{
			ProcessMode = ProcessModeEnum.Always;
			StartButton.Pressed += () =>
			{
				_global.SwitchToStartGame();
			};

			StoreButton.Pressed += () =>
			{
				UiComponent.OpenRuleStoreUi();
			};

			RulesButton.Pressed += () =>
			{
				UiComponent.OpenRuleSetsViewingUi();
			};
		}
	}
}
