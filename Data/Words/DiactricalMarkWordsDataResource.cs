using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

public sealed partial class DiactricalMarkWordsDataResource : Resource
{
	[Export]
	public Dictionary<DiactricalMarkRuleType, Array<DiactricalMarkWordResource>> MarkedWordsByRule { get; set; } = [];
	[Export]
	public Array<DiactricalMarkWordResource> NotMarkedWords { get; set; } = [];
}
