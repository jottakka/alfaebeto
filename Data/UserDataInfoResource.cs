using AlfaEBetto.Data.Rules;
using AlfaEBetto.Data.Rules.Rules;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;
using WordProcessing.Models.SpellingRules;

namespace AlfaEBetto.Data;

[GlobalClass]
public sealed partial class UserDataInfoResource : BaseDataResource
{
	[Export]
	public string UserName { get; set; } = "JOGADOR";
	[Export]
	public long TotalScore { get; set; } = 0;
	[Export]
	public long TotalMoneyAmmount { get; set; } = 0;
	[Export]
	public int TotalGreenKeyGemsAmmount { get; set; } = 0;
	[Export]
	public int TotalRedKeyGemsAmmount { get; set; } = 0;
	[Export]
	public Array<DiactricalMarkRuleType> UnlockedDiactricalMarksSubCategories { get; set; } = [];
	[Export]
	public Array<SpellingRuleRuleType> UnlockedSpellingRuleRuleTypes { get; set; } = [];
	[Export]
	public DiactricalMarkRuleItemResource[] DiactricalMarkRuleItems { get; set; } = System.Array.Empty<DiactricalMarkRuleItemResource>();
	[Export]
	public SpellingRuleRuleItemResource[] SpellingRuleRuleItems { get; set; } = System.Array.Empty<SpellingRuleRuleItemResource>();
	[Export]
	public Dictionary<CategoryType, WordCategoryInfoResource> WordsCategoryInfos { get; set; } = [];

	private readonly WordAccuracyInfoManager _wordAccuracyInfoManager;
	public UserDataInfoResource() => _wordAccuracyInfoManager = new WordAccuracyInfoManager(this);

	public void UpdateWithGameResult(GameResultData gameResultData)
	{
		TotalGreenKeyGemsAmmount += gameResultData.GreenKeyGemsAmmount;
		TotalRedKeyGemsAmmount += gameResultData.RedKeyGemsAmmount;
		TotalMoneyAmmount += gameResultData.MoneyAmmount;
		TotalScore += gameResultData.Score;
		_wordAccuracyInfoManager.UpdateUserWordsResultsData(gameResultData.WordResults);
		_ = EmitSignal(nameof(OnSaveChangesSignal));
	}

	public void Update() => _ = EmitSignal(nameof(OnSaveChangesSignal));
}
