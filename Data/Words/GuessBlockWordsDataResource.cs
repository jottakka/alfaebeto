using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

namespace AlfaEBetto.Data.Words
{
	public sealed partial class GuessBlockWordsDataResource : Resource
	{
		[Export]
		public Dictionary<DiactricalMarkRuleType, Array<GuessBlockWordResource>> GuessBlockWords { get; set; } = [];
	}
}
