using System.IO;
using System.Linq;
using Godot;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;
using WordProcessing.Models.SpellingRules;
using WordProcessing.Processing;

public sealed partial class UnlockableRulesTreeBuilderScript : Node
{
	public override void _Ready()
	{
		GenerateMarkedWords();
		GenerateSpellingRuleWords();
		UserDataInfoResource userDataResource = new();

		RulesResource rulesResource = new()
		{
			DiactricalMarkRuleSets = GenerateDiactricalMarkRuleResources(ref userDataResource),
			SpellingRuleRuleSets = GenerateSpellingRulesResource(ref userDataResource),
		};

		// Save the resource to a .tres file
		string userDataSavePath = "res://SaveFiles/user_data_original.tres";

		Error error = ResourceSaver.Save(userDataResource, userDataSavePath);
		if (error == Error.Ok)
		{
			GD.Print("Resource saved successfully!");
		}
		else
		{
			GD.PrintErr("Failed to save resource: ", error);
		}

		string rulesDataSavePath = "res://SaveFiles/rules_original.tres";

		error = ResourceSaver.Save(rulesResource, rulesDataSavePath);
		if (error == Error.Ok)
		{
			GD.Print("Resource saved successfully!");
		}
		else
		{
			GD.PrintErr("Failed to save resource: ", error);
		}
	}

	private DiactricalMarkRuleSetItemResource[] GenerateDiactricalMarkRuleResources(ref UserDataInfoResource userDataResource)
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
		string jsonString = File.ReadAllText(filePath);

		DiactricalMarkCategories markedWords = MarksJsonDeserializer.DeserializeJsonString(jsonString);
		int genCostIncrement = 2;
		int genCostMultiplier = 0;
		int startGenCost = 3;

		DiactricalMarkRuleSetItemResource[] diactricalMarkRuleSets = markedWords.Categories.Select(
		(c, x) =>
		new DiactricalMarkRuleSetItemResource()
		{
			CategoryType = CategoryType.Acentuation,
			Description = c.Description,
			RuleSet = c.Name,
			RuleSetType = c.Type,
			Rules = [.. c.Subcategories.Select(sc =>
			new DiactricalMarkRuleItemResource
			{
				CategoryType = CategoryType.Acentuation,
				Category = "Acentuação gráfica",
				RuleSetType = c.Type,
				RuleSet = c.Name,
				RuleType = sc.Type,
				Rule = sc.Name,
				Description = sc.Description,
				Examples = sc.Words.Take(3).Select(s => s.Original).ToArray(),
				IsUnlocked = genCostMultiplier == 0,
				KeyGemCost = startGenCost + (genCostMultiplier++ * genCostIncrement)
			})]
		}).ToArray();

		DiactricalMarkRuleItemResource[] flatRuleItems = diactricalMarkRuleSets.SelectMany(r => r.Rules).ToArray();
		userDataResource.UnlockedDiactricalMarksSubCategories.Add(flatRuleItems.First().RuleType);
		userDataResource.DiactricalMarkRuleItems = flatRuleItems;
		return diactricalMarkRuleSets;
	}

	private SpellingRuleRuleSetItemResource[] GenerateSpellingRulesResource(ref UserDataInfoResource userDataInfoResource)
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\meteor_words_data.json";
		string jsonString = File.ReadAllText(filePath);

		SpellingRuleRoot spellingRuleRoot = XorCHDeserializer.DeserializeJsonStringSpellingRule(jsonString);
		int genCostIncrement = 1;
		int genCostMultiplier = 0;
		int startGenCost = 1;

		SpellingRuleRuleSetItemResource[] ruleSets = spellingRuleRoot.RuleCategories.SelectMany(
			(cat) => cat.RuleSets.Select(set =>
			new SpellingRuleRuleSetItemResource()
			{
				CategoryType = cat.RuleCategoryType,
				RuleSet = set.Name,
				RuleSetType = set.RuleSetType,
				Rules = [.. set.Rules.Select(rule =>
					new SpellingRuleRuleItemResource
					{
						CategoryType = cat.RuleCategoryType,
						Category = cat.Description,
						RuleSetType = set.RuleSetType,
						RuleSet = rule.Name,
						RuleType = rule.RuleType,
						Rule = rule.Name,
						Description = rule.Description,
						Examples = rule.Words.Take(3).Select(s => s.Original).ToArray(),
						IsUnlocked = genCostMultiplier == 0,
						KeyGemCost = startGenCost + (genCostMultiplier++ * genCostIncrement)
					})]
			})).ToArray();

		SpellingRuleRuleItemResource[] flatRuleItems = ruleSets.SelectMany(r => r.Rules).ToArray();
		userDataInfoResource.UnlockedSpellingRuleRuleTypes.Add(flatRuleItems.First().RuleType);
		userDataInfoResource.SpellingRuleRuleItems = flatRuleItems;

		return ruleSets;
	}

	private void GenerateMarkedWords()
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
		string jsonString = File.ReadAllText(filePath);

		DiactricalMarkCategories wordsData = MarksJsonDeserializer.DeserializeJsonString(jsonString);

		System.Collections.Generic.Dictionary<DiactricalMarkRuleType, DiactricalMarkWordResource[]> markedWords = wordsData
			.Categories
			.SelectMany(
				(cat, x) =>
					 cat.Subcategories.SelectMany(subCat =>
						 subCat.Words.Select(word =>
							 new DiactricalMarkWordResource
							 {
								 DiactricalMarkSubCategoryType = subCat.Type,
								 Original = word.Original,
								 HasMark = word.HasMark,
								 WithoutMark = word.WithoutDiacritics,
								 MarkIndex = word.DiacriticIndex.Value,
							 })))
			.GroupBy(i => i.DiactricalMarkSubCategoryType)
			.ToDictionary(i => i.Key, i => i.ToArray());

		System.Collections.Generic.IEnumerable<DiactricalMarkWordResource> noMarkWords = wordsData.NotMarkedWords.Select(word =>
			new DiactricalMarkWordResource
			{
				DiactricalMarkSubCategoryType = DiactricalMarkRuleType.SemAcento,
				Original = word.Original,
				HasMark = word.HasMark,
				WithoutMark = word.WithoutDiacritics,
				MarkIndex = -1,
			});

		DiactricalMarkWordsDataResource wordsResource = new()
		{
			NotMarkedWords = [.. noMarkWords],
		};

		foreach ((DiactricalMarkRuleType type, DiactricalMarkWordResource[] words) in markedWords)
		{
			wordsResource.MarkedWordsByRule.Add(type, [.. words]);
		}
		// Save the resource to a .tres file
		string userDataSavePath = "res://SaveFiles/words_data.tres";

		Error error = ResourceSaver.Save(wordsResource, userDataSavePath);
		if (error == Error.Ok)
		{
			GD.Print("Resource saved successfully!");
		}
		else
		{
			GD.PrintErr("Failed to save resource: ", error);
		}
	}

	private void GenerateSpellingRuleWords()
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\meteor_words_data.json";
		string jsonString = File.ReadAllText(filePath);

		SpellingRuleRoot spellingRuleRoot = XorCHDeserializer.DeserializeJsonStringSpellingRule(jsonString);

		System.Collections.Generic.Dictionary<SpellingRuleRuleType, SpellingRuleWordResource[]> spellingRuleWords = spellingRuleRoot
			.RuleCategories
			.SelectMany(cat =>
				cat.RuleSets
				.SelectMany(
					(ruleSet, x) =>
						 ruleSet.Rules.SelectMany(rule =>
							 rule.Words.Select(word =>
								 new SpellingRuleWordResource
								 {
									 CategoryType = cat.RuleCategoryType,
									 SpellingRuleType = rule.RuleType,
									 Options = word.Options.ToArray(),
									 Original = word.Original,
									 RightOption = word.RightOption,
									 FirstPart = word.FirstPart,
									 SecondPart = word.SecondPart,
								 }))))
			.GroupBy(word => word.SpellingRuleType)
			.ToDictionary(i => i.Key, i => i.ToArray());

		SpellingRulesResource wordsResource = new();
		foreach ((SpellingRuleRuleType type, SpellingRuleWordResource[] words) in spellingRuleWords)
		{
			wordsResource.WordsByRule.Add(type, [.. words]);
		}
		// Save the resource to a .tres file
		string userDataSavePath = "res://SaveFiles/spelling_rule_words_data.tres";

		Error error = ResourceSaver.Save(wordsResource, userDataSavePath);
		if (error == Error.Ok)
		{
			GD.Print("Resource saved successfully!");
		}
		else
		{
			GD.PrintErr("Failed to save resource: ", error);
		}
	}
}
