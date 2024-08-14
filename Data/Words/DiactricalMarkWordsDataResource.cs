using Godot;
using Godot.Collections;

public sealed partial class DiactricalMarkWordsDataResource : Resource
{
    [Export]
    public Dictionary<int, Array<DiactricalMarkWordResource>> MarkedWordsByRule { get; set; } = new();
    [Export]
    public Array<DiactricalMarkWordResource> NotMarkedWords { get; set; } = new();
}
