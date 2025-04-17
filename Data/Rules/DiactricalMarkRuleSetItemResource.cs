using System.Linq;
using AlfaEBetto.Data.Rules.Rules;
using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

namespace AlfaEBetto.Data.Rules
{
	public sealed partial class DiactricalMarkRuleSetItemResource : BaseRuleSetItemResource
	{
		[Export]
		public DiactricalMarkRuleSetType RuleSetType { get; set; }
		[Export]
		public Array<DiactricalMarkRuleItemResource> Rules { get; set; }

		public override BaseRuleItemResource[] RulesAsBaseItemResource =>
			Rules.Select(r => r as BaseRuleItemResource).ToArray();
	}
}
