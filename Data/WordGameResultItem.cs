using WordProcessing.Models.Rules;

namespace AlfaEBetto.Data
{
	public sealed record WordGameResultItem
	{
		public CategoryType RuleType { get; set; }

		public string Word { get; set; }

		public int Errors { get; set; } = 0;

		public int Successes { get; set; } = 0;
	}
}
