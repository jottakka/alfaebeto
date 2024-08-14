using Godot;

public sealed class LetterBlockBuilder
{
    private readonly PackedScene _letterBlockPackedScene;
    private readonly PackedScene _noLetterBlockPackedScene;

    public LetterBlockBuilder(
        PackedScene letterBlockPackedScene,
        PackedScene noLetterBlockPackedScene
    )
    {
        _letterBlockPackedScene = letterBlockPackedScene;
        _noLetterBlockPackedScene = noLetterBlockPackedScene;
    }

    public LetterBlock BuildLetterBlock(char letter, Vector2 position, bool isTarget)
    {
        LetterBlock letterBlock = _letterBlockPackedScene.Instantiate<LetterBlock>();
        letterBlock.SetLabel(letter);
        letterBlock.SetPosition(position);
        letterBlock.IsTarget = isTarget;

        return letterBlock;
    }

    public NoLetterBlock BuildNoLetterBlock(Vector2 position, bool isTarget)
    {
        NoLetterBlock noLetterBlock = _noLetterBlockPackedScene.Instantiate<NoLetterBlock>();
        noLetterBlock.SetPosition(position);
        noLetterBlock.IsTarget = isTarget;

        return noLetterBlock;
    }
}
