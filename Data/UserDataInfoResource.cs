using AlfaEBetto.Data;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;

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
    public Array<DiactricalMarkSubCategoryType> UnlockedDiactricalMarksSubCategories { get; set; } = new();
    [Export]
    public DiactricalMarkRuleItemResource[] DiactricalMarkRuleItems { get; set; } = new DiactricalMarkRuleItemResource[0];
    [Export]
    public Dictionary<RuleType, WordCategoryInfoResource> WordsCategoryInfos { get; set; } = new();

    private readonly WordAccuracyInfoManager _wordAccuracyInfoManager;
    public UserDataInfoResource()
    {
        _wordAccuracyInfoManager = new WordAccuracyInfoManager(this);
    }

    public void UpdateWithGameResult(GameResultData gameResultData)
    {
        TotalGreenKeyGemsAmmount += gameResultData.GreenKeyGemsAmmount;
        TotalRedKeyGemsAmmount += gameResultData.RedKeyGemsAmmount;
        TotalMoneyAmmount += gameResultData.MoneyAmmount;
        TotalScore += gameResultData.Score;
        _wordAccuracyInfoManager.UpdateUserWordsResultsData(gameResultData.WordResults);
    }
}
