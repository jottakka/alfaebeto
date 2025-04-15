using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.XorCH;

namespace WordProcessing.Filtering;

public static class DiacriticalWordFiltering
{
	public static IEnumerable<DiactricalMarkWordInfo> GetWordInfosBySubCategories(
		this DiactricalMarkCategories diactricalMarkCategories,
		DiactricalMarkRuleType[] subCategories)
	{
		IEnumerable<DiactricalMarkWordInfo> words = diactricalMarkCategories.Categories
			.SelectMany(cat => cat.Subcategories)
			.Where(subCat => subCategories.Contains(subCat.Type))
			.SelectMany(s => s.Words);

		return words;
	}

	public static IEnumerable<XorCHWord> GetWordInfosByCategories(
		this XorCHRules xorCHRules,
		IEnumerable<RuleType>? subCategories = null,
		int take = 10)
	{
		IEnumerable<XorCHWord> shuffedWords = xorCHRules
			.Rules
			.SelectMany(r => r.Words);

		return shuffedWords;
	}
}