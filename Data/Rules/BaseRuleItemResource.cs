using Godot;
using WordProcessing.Models.Rules;

public partial class BaseRuleItemResource
	: Resource
{
	[Export]
	public CategoryType CategoryType { get; set; }
	[Export]
	public string Category { get; set; }
	[Export]
	public string RuleSet { get; set; }
	[Export]
	public string Rule { get; set; }
	[Export]
	public string Description { get; set; }
	[Export]
	public string[] Examples { get; set; }
	[Export]
	public bool IsUnlocked { get; set; }
	[Export]
	public int KeyGemCost { get; set; } = 5;

	[Signal]
	public delegate void OnUnlockSignalEventHandler();

	public void Unlock()
	{
		IsUnlocked = true;
		_ = EmitSignal(nameof(OnUnlockSignal));
	}
}
