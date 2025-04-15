using WordProcessing.Processing;

namespace WordProcessing.Test;

public class UnitTest1
{
	[Fact]
	public void Test1()
	{
		// Path to your JSON file
		string filePath = "C:\\git\\alfa_e_betto\\Data\\acentua��o\\acentos_dados.json";

		// Read the JSON file as a string
		string jsonString = File.ReadAllText(filePath);
		Models.DiacriticalMarks.DiactricalMarkCategories values = MarksJsonDeserializer.DeserializeJsonString(jsonString);
		IReadOnlyList<Models.Rules.RuleSetListItemViewModel> test = MarksWordsToListViewModel.Convert(values.Categories);
		Console.WriteLine(test.Count());
	}

	[Fact]
	public void Test2()
	{
		// Path to your JSON file
		string filePath = @"C:\git\alfa_e_betto\Data\acentua��o\meteor_words_data.json";

		// Read the JSON file as a string
		string jsonString = File.ReadAllText(filePath);
		Models.SpellingRules.SpellingRuleRoot? test = XorCHDeserializer.DeserializeJsonStringSpellingRule(jsonString);

		Assert.True(test is not null);
	}
}