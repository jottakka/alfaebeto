using System.IO;
using Godot;

public sealed class DataResourceManager
{
	private const string _saveDataBasePath = "res://SaveFiles/";
	private const string _userDataFileName = "user_data.tres";
	private const string _userDataOriginalFileName = "user_data_original.tres";
	private const string _rulesOriginalFileName = "rules_original.tres";
	private const string _spellingRuleWordsDataFileName = "spelling_rule_words_data.tres";

	private const string _wordsDataFileName = "words_data.tres";

	private string _userDataFilePath => Path.Combine(_saveDataBasePath, _userDataFileName);
	private string _rulesOriginalFilePath => Path.Combine(_saveDataBasePath, _rulesOriginalFileName);
	private string _userDataOriginalFilePath => Path.Combine(_saveDataBasePath, _userDataOriginalFileName);
	private string _wordsDataOriginalFilePath => Path.Combine(_saveDataBasePath, _wordsDataFileName);
	private string _spellingRuleWordsDataFilePath => Path.Combine(_saveDataBasePath, _spellingRuleWordsDataFileName);

	private readonly RulesResource _rulesOriginalResource;
	private readonly UserDataInfoResource _userDataOriginalInfoResource;

	public RulesResource RulesResource { get; }

	public UserDataInfoResource UserDataInfoResource { get; }

	public SpellingRulesResource SpellingRulesResource { get; }

	public DiactricalMarkWordsDataResource DiactricalMarkWordsDataResource { get; }

	public DataResourceManager()
	{
		_userDataOriginalInfoResource = ResourceLoader.Load<UserDataInfoResource>(_userDataOriginalFilePath);

		DiactricalMarkWordsDataResource = ResourceLoader.Load<DiactricalMarkWordsDataResource>(_wordsDataOriginalFilePath);

		SpellingRulesResource = ResourceLoader.Load<SpellingRulesResource>(_spellingRuleWordsDataFilePath);

		RulesResource = ResourceLoader.Load<RulesResource>(_rulesOriginalFilePath);
		UserDataInfoResource = LoadUserDataResource();

		UserDataInfoResource.OnSaveChangesSignal += () => SaveResource(UserDataInfoResource, _userDataFilePath);
	}

	private UserDataInfoResource LoadUserDataResource()
	{
		if (ResourceLoader.Exists(_userDataFilePath) is false)
		{
			Resource originalDuplicate = _userDataOriginalInfoResource.Duplicate(true);
			SaveResource(originalDuplicate, _userDataFilePath);
		}

		UserDataInfoResource userData = ResourceLoader.Load<UserDataInfoResource>(_userDataFilePath);

		ConnectRuleBoughtSignalToDiactricalRules(userData);

		ConnectRuleBoughtSignalToSpellingRules(userData);

		return userData;
	}

	private static void ConnectRuleBoughtSignalToSpellingRules(UserDataInfoResource userData)
	{
		foreach (SpellingRuleRuleItemResource rule in userData.SpellingRuleRuleItems)
		{
			if (rule.IsUnlocked is false)
			{
				rule.OnUnlockSignal += () =>
				{
					userData.UnlockedSpellingRuleRuleTypes.Add(rule.RuleType);
					userData.Update();
				};
			}
		}
	}

	private static void ConnectRuleBoughtSignalToDiactricalRules(UserDataInfoResource userData)
	{
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
