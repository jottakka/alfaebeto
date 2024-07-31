using Godot;

public sealed class LetterBlockBuilder
{
    private readonly PackedScene _letterBlockPackedScene;

    public LetterBlockBuilder(PackedScene packedScene)
    {
        _letterBlockPackedScene = packedScene;
    }

    public LetterBlock BuildLetterBlock(char letter, Vector2 position, bool isTarget)
    {
        LetterBlock letterBlock = _letterBlockPackedScene.Instantiate<LetterBlock>();
        letterBlock.SetLabel(letter);
        letterBlock.SetPosition(position);
        letterBlock.IsTarget = isTarget;

        return letterBlock;
    }
}
