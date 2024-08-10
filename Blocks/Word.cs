using System.Collections.Generic;
using System.Linq;
using Godot;
using WordProcessing.Models.DiacriticalMarks;

public partial class Word : Node2D
{
	[Export]
	public PackedScene LetterBlockPackedScene { get; set; }
	[Signal]
	public delegate void ReadyToDequeueSignalEventHandler();
	[Signal]
	public delegate void OnLetterDestructedSignalEventHandler(bool isTarget);

	[Signal]
	public delegate void OnDisableChildrenCollisionsInternalSingalEventHandler();

	public DiactricalMarkWordInfo WordInfo { get; set; }

	public float CenterOffset { get; set; } = 0.0f;

	public int TargetIdx { get; set; }

	public Queue<LetterBlock> LetterBlocks { get; set; } = new();

	public LetterBlock Target { get; set; }

	private static LetterBlockBuilder _letterBuilder = null;

	private Timer _destructionTimer = new();
	public override void _Ready()
	{
		_letterBuilder ??= new LetterBlockBuilder(LetterBlockPackedScene);
		AddChild(_destructionTimer);
		BuildLetterBlocks();
		_destructionTimer.Timeout += () =>
		{
			if (LetterBlocks.Any())
			{
				LetterBlocks.Dequeue().Destroy();
			}
			else
			{
				_destructionTimer.Stop();
			}
		};
	}

	private void BuildLetterBlocks()
	{
		//TO DO try to improve using a margin container
		// Start position for the first letter
		float currentX = 0.0f;
		// Aggregate function to place each letter based on the previous one's position and width
		foreach ((char letter, int idx) in WordInfo.WithoutDiacritics.Select((letter, idx) => (letter, idx)))
		{
			currentX = BuildLetterBlock(currentX, letter, idx);
		}

		LetterBlock lastBlock = LetterBlocks.Last();
		lastBlock.OnReadyToDequeueSignal += () =>
		{
			_ = EmitSignal(nameof(ReadyToDequeueSignal));
		};

		CenterOffset = currentX / 2.0f;

		Position -= new Vector2(CenterOffset, 0);

		Target.OnTargetBlockCalledDestructionSignal += Destroy;
	}

	private float BuildLetterBlock(float currentX, char letter, int idx)
	{
		bool isTarget = WordInfo.DiacriticIndex == idx;

		LetterBlock letterBlock = _letterBuilder.BuildLetterBlock(
			letter,
			new Vector2(currentX, 0),
			isTarget: isTarget
		);

		if (isTarget)
		{
			Target = letterBlock;
		}
		else
		{
			OnDisableChildrenCollisionsInternalSingal += letterBlock.DisableCollisions;
		}

		letterBlock.OnLetterDestructedSignal += OnLetterDestroyed;

		CollisionShape2D collisionShape = letterBlock
			.CollisionShape;
		LetterBlocks.Enqueue(letterBlock);
		RectangleShape2D shape = collisionShape.Shape as RectangleShape2D;
		currentX += shape.Size.X * 2;
		AddChild(letterBlock);
		return currentX;
	}

	private void OnLetterDestroyed(bool isTarget)
	{
		if (isTarget)
		{
			_ = EmitSignal(nameof(OnDisableChildrenCollisionsInternalSingal));
		}

		_ = EmitSignal(nameof(OnLetterDestructedSignal), isTarget);
	}

	public void Destroy()
	{
		LetterBlocks.Dequeue().Destroy();
		_destructionTimer.Start(0.25f);
	}
}

