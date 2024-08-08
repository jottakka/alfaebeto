using System.Collections.Generic;
using System.IO;
using Godot;
using WordProcessing.Filtering;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;
using WordProcessing.Models.XorCH;
using WordProcessing.Processing;

public partial class Global : Node
{
	public static Global Instance { get; private set; } = null;

	public Player Player { get; private set; }

	public StageBase Scene { get; private set; }

	public Queue<WordInfo> MarkedWords { get; private set; }

	public Queue<XorCHWord> XorChWords { get; private set; }

	public IReadOnlyList<RuleSetListItemViewModel> RuleSetListItems { get; private set; }

	[Signal]
	public delegate void OnMainNodeSetupFinishedSignalEventHandler();

	public Global()
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
		string jsonString = File.ReadAllText(filePath);

		DiactricalMarkCategories markedWords = MarksJsonDeserializer.DeserializeJsonString(jsonString);
		MarkedWords = markedWords
			.GetWordsShuffledByCategory(CategoryEnum.Paroxitonas);

		filePath = @"C:\git\alfa_e_betto\Data\acentuação\ch_e_x_proper.json";
		jsonString = File.ReadAllText(filePath);
		XorChWords = XorCHDeserializer
			.DeserializeJsonString(jsonString)
			.GetXorCHWordsShuffled();

		RuleSetListItems = MarksWordsToListViewModel.Convert(markedWords.Categories);
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
	}

	public void SettingMainNodeData(Player player, StageBase stage)
	{
		Player = player;
		Scene = stage;
		_ = EmitSignal(nameof(OnMainNodeSetupFinishedSignal));
	}
}
