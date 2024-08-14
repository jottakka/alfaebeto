using Godot;
using Godot.Collections;
using WordProcessing.Models.DiacriticalMarks;

public sealed partial class DiactricalMarkWordsDataResource : Resource
{
    [Export]
    public Dictionary<DiactricalMarkSubCategoryType, Array<DiactricalMarkWordResource>> MarkedWordsByRule { get; set; } = new();
    [Export]
    public Array<DiactricalMarkWordResource> NotMarkedWords { get; set; } = new();
}
