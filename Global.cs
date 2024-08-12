using System.Collections.Generic;
using Godot;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.XorCH;

public partial class Global : Node
{
	public static Global Instance { get; private set; } = null;

	public UserDataManagementComponent UserDataManagementComponent { get; set; }
	public UserDataInfoResource UserDataInfoResource => _dataResourceManager.UserDataInfoResource;

	public RulesResource RulesResource => _dataResourceManager.RulesResource;

	public Player Player { get; private set; }

	public StageBase Scene { get; private set; }

	public Queue<DiactricalMarkWordInfo> MarkedWords { get; private set; }

	public Queue<XorCHWord> XorChWords { get; private set; }

	public WordServerManager WordServerManager { get; } = new();

	[Signal]
	public delegate void OnMainNodeSetupFinishedSignalEventHandler();

	private DataResourceManager _dataResourceManager { get; } = new();

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
