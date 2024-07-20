using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Word : Node2D
{
	[Export]
	public PackedScene LetterBlockPackedScene { get; set; }
	[Export]
	public string WordText { get; set; } = "TESTE";
	public IList<LetterBlock> LetterBlocks { get; set; } = new List<LetterBlock>();

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

		// Aggregate function to place each letter based on the previous one's position and width
		foreach (var letter in WordText)
		{
			var letterBlock = _letterBuilder.BuildLetterBlock(
				letter,
				new Vector2(currentX, 0)
			);
			AddChild(letterBlock);
			var collisionShape = letterBlock
				.GetChildren()
				.OfType<CollisionShape2D>()
				.Single();
			var shape = collisionShape.Shape as RectangleShape2D;
			currentX += shape.Size.X * 2;
		}
	}
}

