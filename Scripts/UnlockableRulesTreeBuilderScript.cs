using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Processing;

public sealed partial class UnlockableRulesTreeBuilderScript : Node
{
	public override void _Ready()
	{
		GenerateMakredWords();
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
		string jsonString = File.ReadAllText(filePath);

		DiactricalMarkCategories markedWords = MarksJsonDeserializer.DeserializeJsonString(jsonString);
		UserDataInfoResource userDataResource = new();
		int genCostIncrement = 1;
		int genCostMultiplier = 0;
		int startGenCost = 3;

		RulesResource rulesResource = new()
		{
			DiactricalMarkRuleSets = markedWords.Categories.Select(
			(c, x) =>
			new DiactricalMarkRuleSetItemResource()
			{
				Description = c.Description,
				Name = c.Name,
				RuleSetType = c.Type,
				Rules = c.Subcategories.Select(sc =>
				new DiactricalMarkRuleItemResource
				{
					RuleSetType = c.Type,
					RuleSet = c.Name,
					RuleType = sc.Type,
					Name = sc.Name,
					Description = sc.Description,
					Examples = sc.Words.Take(3).Select(s => s.Original).ToArray(),
					IsUnlocked = genCostMultiplier == 0,
					KeyGemCost = startGenCost + (genCostMultiplier++ * genCostIncrement)
				}).ToArray()
			}).ToArray()
		};
		DiactricalMarkRuleItemResource[] flatRuleItems = rulesResource.DiactricalMarkRuleSets.SelectMany(r => r.Rules).ToArray();
		userDataResource.UnlockedDiactricalMarksSubCategories.Add(flatRuleItems.First().RuleType);
		userDataResource.DiactricalMarkRuleItems = flatRuleItems;
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

	private void GenerateMakredWords()
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
		string jsonString = File.ReadAllText(filePath);

		DiactricalMarkCategories wordsData = MarksJsonDeserializer.DeserializeJsonString(jsonString);

		System.Collections.Generic.Dictionary<DiactricalMarkSubCategoryType, DiactricalMarkWordResource[]> markedWords = wordsData
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
				DiactricalMarkSubCategoryType = DiactricalMarkSubCategoryType.SemAcento,
				Original = word.Original,
				HasMark = word.HasMark,
				WithoutMark = word.WithoutDiacritics,
				MarkIndex = -1,
			});

		DiactricalMarkWordsDataResource wordsResource = new()
		{
			NotMarkedWords = new Array<DiactricalMarkWordResource>(noMarkWords),
		};

		foreach ((DiactricalMarkSubCategoryType type, DiactricalMarkWordResource[] words) in markedWords)
		{
			wordsResource.MarkedWordsByRule.Add((int)type, new Array<DiactricalMarkWordResource>(words));
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
}
