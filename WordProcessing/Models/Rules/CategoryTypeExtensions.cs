namespace WordProcessing.Models.Rules;
public static class CategoryTypeExtensions
{
	public static string GetCategoryName(this CategoryType category)
	{
		return category switch
		{
			CategoryType.Acentuation => "Acentuação Gráfica",
			CategoryType.SorZ => "Uso de S ou Z",
			CategoryType.XorCH => "Uso de X ou CH",
			_ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
		};
	}
}
