using AlfaEBetto.Blocks;
using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Components;

public partial class WordBuilderComponent : Node
{
	[Export]
	public PackedScene WordPackedScene { get; set; }

	private static WordBuilder _wordBuilder = null;

	public override void _Ready() => _wordBuilder ??= new WordBuilder(WordPackedScene);

	public Word BuildWord(DiactricalMarkWordResource wordInfo, Vector2 position)
	{
		Word word = _wordBuilder.BuildWord(wordInfo, position);
		return word;
	}
}
