using Godot;
using WordProcessing.Models.Rules;

public partial class WordAccuracyInfo : Resource
{
    [Export]
    public string Word { get; set; }
    [Export]
    public int Errors { get; set; } = 0;
    [Export]
    public int Successes { get; set; } = 0;
    [Export]
    public RuleType RuleType { get; set; }
}
