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
		userDataResource.DiactricalMarkRuleItems = new Godot.Collections.Array<DiactricalMarkRuleItemResource>(markedWords.Categories.SelectMany(
			(c, x) =>
			c.Subcategories.Select(sc =>
				new DiactricalMarkRuleItemResource
				{
					RuleSetType = c.Type,
					RuleSet = c.Name,
					SubRuleType = sc.Type,
					SubRule = sc.Name,
					IsUnlocked = genCostMultiplier == 0,
					KeyGemCost = startGenCost + (genCostMultiplier++ * genCostIncrement)
				})));
		userDataResource.UnlockedDiactricalMarksSubCategories.Add(userDataResource.DiactricalMarkRuleItems.First().SubRuleType);

		// Define the path where you want to save the resource
		string savePath = "res://SaveFiles/dmarks_unlocks.tres";

		// Save the resource to a .tres file
		Error error = ResourceSaver.Save(userDataResource, savePath);
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
