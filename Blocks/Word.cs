using Godot;
using System.Collections.Generic;
using System.Linq;
using WordProcessing.Models.DiacriticalMarks;

public partial class Word : Node2D
{
    [Export]
    public PackedScene LetterBlockPackedScene { get; set; }
    [Signal]
    public delegate void OnTargetBlockDestructedSignalEventHandler();

    public WordInfo WordInfo { get; set; }

    public float CenterOffset { get; set; } = 0.0f;

    public int TargetIdx { get; set; }

    public IList<LetterBlock> LetterBlocks { get; set; }

    public LetterBlock Target { get; set; }


    private static LetterBlockBuilder _letterBuilder = null;

    public override void _Ready()
    {
        if (_letterBuilder == null)
        {
            _letterBuilder = new LetterBlockBuilder(LetterBlockPackedScene);
        }
        BuildLetterBlocks();
    }

    private void BuildLetterBlocks()
    {
        // Start position for the first letter
        float currentX = 0.0f;
        LetterBlocks = new List<LetterBlock>();
        // Aggregate function to place each letter based on the previous one's position and width
        foreach (var (letter, idx) in WordInfo.WithoutDiacritics.Select((letter, idx) => (letter, idx)))
        {
            var isTarget = WordInfo.DiacriticIndex == idx;

            var letterBlock = _letterBuilder.BuildLetterBlock(
                letter,
                new Vector2(currentX, 0),
                isTarget: isTarget
            );

            if (isTarget)
            {
                Target = letterBlock;
            }

            AddChild(letterBlock);
            var collisionShape = letterBlock
                .CollisionShape;
            LetterBlocks.Add(letterBlock);
            var shape = collisionShape.Shape as RectangleShape2D;
            currentX += shape.Size.X * 2;
        }
        CenterOffset = currentX / 2.0f;
        Position -= new Vector2(CenterOffset, 0);

        Target.OnTargetDestructedSignal += () =>
        {
            EmitSignal(nameof(OnTargetBlockDestructedSignal));
        };
    }
}

