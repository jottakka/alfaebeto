using System.Collections.Generic;
using Godot;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;
using WordProcessing.Models.XorCH;

public partial class Global : Node
{
	[Export]
	public UserDataManagementComponent UserDataManagementComponent { get; set; }

	public static Global Instance { get; private set; } = null;

	public Player Player { get; private set; }

	public StageBase Scene { get; private set; }

	public Queue<DiactricalMarkWordInfo> MarkedWords { get; private set; }

	public Queue<XorCHWord> XorChWords { get; private set; }

	public IReadOnlyList<RuleSetListItemViewModel> RuleSetListItems { get; private set; }

	public WordServerManager WordServerManager { get; private set; } = new();

	[Signal]
	public delegate void OnMainNodeSetupFinishedSignalEventHandler();

	public Global()
	{
		RuleSetListItems = new List<RuleSetListItemViewModel>();
	}

	public override void _Ready()
	{
		if (Instance is not null)
		{
			QueueFree();
			return;
		}

		Instance = this;
		// Initialize random number generator
		GD.Randomize();
		XorChWords = WordServerManager.GetShuffledXorCHWords();
		MarkedWords = WordServerManager.GetShuffledDiactricalMarkWords();
	}

	public void SettingMainNodeData(Player player, StageBase stage)
	{
		Player = player;
		Scene = stage;
		_ = EmitSignal(nameof(OnMainNodeSetupFinishedSignal));
	}
}
