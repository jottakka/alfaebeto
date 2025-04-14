using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ArticlesSet : Node2D
{
	[Export]
	public PackedScene LetterBlockPackedScene { get; set; }
	[Signal]
	public delegate void ReadyToDequeueSignalEventHandler();
	[Signal]
	public delegate void OnLetterDestructedSignalEventHandler(bool isTarget);
	[Signal]
	public delegate void OnDisableChildrenCollisionsInternalSingalEventHandler();

	public GuessBlockWordResource GuessBlockInfo { get; set; }

	public float CenterOffset { get; set; } = 0.0f;

	public int TargetIdx { get; set; }

	public Queue<LetterBlock> LetterBlocks { get; set; } = new();

	public LetterBlock Target { get; set; }

	public int NumberOfIncorrectOptions { get; set; }

	private static LetterBlockBuilder _letterBuilder = null;

	private Timer _destructionTimer = new();
	public override void _Ready()
	{
		_letterBuilder ??= new LetterBlockBuilder(LetterBlockPackedScene, null);
		AddChild(_destructionTimer);
		BuildWordBlocks();
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

	private void BuildWordBlocks()
	{
		float currentX = 0;

		foreach ((string word, int idx) in GuessBlockInfo.ShuffledOptions.Select((letter, idx) => (letter, idx)))
		{
			currentX = BuildWordBlock(currentX, word, idx, GuessBlockInfo.AnswerIdx);
		}

		Target.OnTargetBlockCalledDestructionSignal += Destroy;
		CenterWordPositionOn(currentX);
	}

	private void CenterWordPositionOn(float currentX)
	{
		CenterOffset = currentX / 2.0f;

		Position -= new Vector2(CenterOffset, 0);
	}

	private float BuildWordBlock(float currentX, string word, int idx, int ansIdx)
	{
		bool isTarget = ansIdx == idx;

		LetterBlock letterBlock = _letterBuilder.BuildLetterBlock(
			word,
			new Vector2(currentX, 0),
			isTarget: isTarget
		);

		if(idx == 0)
		{
			letterBlock.OnReadyToDequeueSignal += () =>
			{
				_ = EmitSignal(nameof(ReadyToDequeueSignal));
			};

		}

		return SetBlock(letterBlock, currentX);
	}

	private float SetBlock(LetterBlock letterBlock, float currentX)
	{
		if (letterBlock.IsTarget)
		{
			Target = letterBlock;
		}
		else
		{
			OnDisableChildrenCollisionsInternalSingal += letterBlock.DisableCollisions;
		}

		letterBlock.OnLetterDestructedSignal += OnLetterDestroyed;

		LetterBlocks.Enqueue(letterBlock);
		AddChild(letterBlock);

		return CalculateNextLetterPosition(letterBlock, currentX);
	}

	private static float CalculateNextLetterPosition(LetterBlock letterBlock, float currentX)
	{
		CollisionShape2D collisionShape = letterBlock.CollisionShape;

		RectangleShape2D shape = collisionShape.Shape as RectangleShape2D;

		float nextXPosition = currentX + (shape.Size.X * 2);
		return nextXPosition;
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
