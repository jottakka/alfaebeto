using System.IO;
using Godot;

public sealed class DataResourceManager
{
    private const string _saveDataBasePath = "res://SaveFiles/";
    private const string _userDataFileName = "user_data.tres";
    private const string _rulesFileName = "rules.tres";
    private const string _userDataOriginalFileName = "user_data_original.tres";
    private const string _rulesOriginalFileName = "rules_original.tres";

    private const string _wordsDataFileName = "words_data.tres";

    private string _rulesFilePath => Path.Combine(_saveDataBasePath, _rulesFileName);
    private string _userDataFilePath => Path.Combine(_saveDataBasePath, _userDataFileName);
    private string _rulesOriginalFilePath => Path.Combine(_saveDataBasePath, _rulesOriginalFileName);
    private string _userDataOriginalFilePath => Path.Combine(_saveDataBasePath, _userDataOriginalFileName);
    private string _wordsDataOriginalFilePath => Path.Combine(_saveDataBasePath, _wordsDataFileName);

    private readonly RulesResource _rulesOriginalResource;
    private readonly UserDataInfoResource _userDataOriginalInfoResource;

    public RulesResource RulesResource { get; }

    public UserDataInfoResource UserDataInfoResource { get; }

    public DiactricalMarkWordsDataResource DiactricalMarkWordsDataResource { get; }

    public DataResourceManager()
    {
        _rulesOriginalResource = ResourceLoader.Load<RulesResource>(_rulesOriginalFilePath);
        _userDataOriginalInfoResource = ResourceLoader.Load<UserDataInfoResource>(_userDataOriginalFilePath);

        DiactricalMarkWordsDataResource = ResourceLoader.Load<DiactricalMarkWordsDataResource>(_wordsDataOriginalFilePath);

        RulesResource = LoadRuleResource();
        UserDataInfoResource = LoadUserDataResource();

        RulesResource.OnSaveChangesSignal += () => SaveResource(RulesResource, _rulesFilePath);
        UserDataInfoResource.OnSaveChangesSignal += () => SaveResource(UserDataInfoResource, _userDataFilePath);
    }

    private RulesResource LoadRuleResource()
    {
        if (ResourceLoader.Exists(_rulesFilePath) is false)
        {
            Resource originalDuplicate = _rulesOriginalResource.Duplicate(true);
            SaveResource(originalDuplicate, _rulesFilePath);
        }

        return ResourceLoader.Load<RulesResource>(_rulesFilePath);
    }

    private UserDataInfoResource LoadUserDataResource()
    {
        if (ResourceLoader.Exists(_userDataFilePath) is false)
        {
            Resource originalDuplicate = _userDataOriginalInfoResource.Duplicate(true);
            SaveResource(originalDuplicate, _userDataFilePath);
        }

        UserDataInfoResource userData = ResourceLoader.Load<UserDataInfoResource>(_userDataFilePath);

        foreach (DiactricalMarkRuleItemResource rule in userData.DiactricalMarkRuleItems)
        {
            if (rule.IsUnlocked is false)
            {
                rule.OnUnlockSignal += () =>
                {
                    userData.UnlockedDiactricalMarksSubCategories.Add(rule.RuleType);
                    userData.Update();
                };
            }
        }

        return userData;
    }

    private void SaveResource(Resource resource, string path)
    {
        Error error = ResourceSaver.Save(resource, path);
        if (error != Error.Ok)
        {
            GD.PrintErr("Failed to save resource: ", error);
        }
    }
}
