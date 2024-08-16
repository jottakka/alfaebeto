using System;
using System.Linq;
using Godot;
using WordProcessing.Models.Rules;

public partial class BaseRuleSetItemResource : Resource
{
	[Export]
	public CategoryType CategoryType { get; set; }
	[Export]
	public string RuleSet { get; set; }
	[Export]
	public string Description { get; set; }

	public virtual BaseRuleItemResource[] RulesAsBaseItemResource => throw new NotImplementedException("This resource should not be used, only its derived classes");

	public int UnlockedRulesCount => RulesAsBaseItemResource.Count(rule => rule.IsUnlocked);

	public int TotalRulesCount => RulesAsBaseItemResource.Count();
}
