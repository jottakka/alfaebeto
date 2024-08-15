using Godot;
using WordProcessing.Models.Rules;

[GlobalClass]
public partial class WordAccuracyInfoResource : Resource
{
    [Export]
    public CategoryType RuleType { get; set; }
    [Export]
    public string Word { get; set; }
    [Export]
    public int Errors { get; set; } = 0;
    [Export]
    public int Successes { get; set; } = 0;
}
