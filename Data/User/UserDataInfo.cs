using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;

public partial class UserDataInfo : Resource
{
    [Export]
    public string UserName { get; set; }
    [Export]
    public Array<DiactricalMarkSubCategoryType> UnlockedDiactricalMarksSubCategories { get; set; } = new();
    [Export]
    public Dictionary<RuleType, WordCategoryInfo> WordsCategoryInfos { get; set; } = new();
}
