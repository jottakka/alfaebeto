using Godot;

public sealed class LetterBlockBuilder
{
	private PackedScene _letterBlockPackedScene;

	public LetterBlockBuilder(PackedScene packedScene)
	{
		_letterBlockPackedScene = packedScene;
	}

	public LetterBlock BuildLetterBlock(char letter, Vector2 position)
	{
		LetterBlock letterBlock = _letterBlockPackedScene.Instantiate<LetterBlock>();
		letterBlock.SetLabel(letter);
		letterBlock.SetPosition(position);
		return letterBlock;
	}
}
