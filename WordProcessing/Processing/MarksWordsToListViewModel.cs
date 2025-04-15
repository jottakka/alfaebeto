using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.Rules;

namespace WordProcessing.Processing;
public sealed class MarksWordsToListViewModel
{
	public static IReadOnlyList<RuleSetListItemViewModel> Convert(
		IEnumerable<DiactralMarkCategory> categories
		)
	{
		return categories.Select(
			cat => new RuleSetListItemViewModel(
			RuleSetName: cat.Name,
			TotalCount: cat.Subcategories.Count(),
			UnlockedCount: 0,
			Rules: SubcategoriesToRuleListItemViewModel(cat.Subcategories),
			RichTextDescription: cat.Description
		   )).ToList();
	}

	private static DetailedRuleViewModel SubcategoryToDetailedRuleViewMode(DiactricalMarkSubCategory subcategory)
	{
		return new DetailedRuleViewModel(
			RuleName: subcategory.Name,
			RuleDescriptionRichText: subcategory.Description,
			Examples: subcategory.Words.Take(3).Select(w => w.Original).ToArray(),
			IsUnlocked: false,
			RuleType: CategoryType.Acentuation
		);
	}

	private static IReadOnlyList<RuleListItemViewModel> SubcategoriesToRuleListItemViewModel(IEnumerable<DiactricalMarkSubCategory> subcategories)
	{
		return subcategories.Select(
			subCat => new RuleListItemViewModel(
				Name: subCat.Name,
				DetailedModel: SubcategoryToDetailedRuleViewMode(subCat),
				RichTextDescription: subCat.Description
			)).ToList();
	}
}