using System.IO;
using System.Linq;
using Godot;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Processing;

public sealed partial class UnlockableRulesTreeBuilderScript : Node
{
	public override void _Ready()
	{
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
}
