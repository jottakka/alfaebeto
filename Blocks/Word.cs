using System.Collections.Generic;
using System.Linq;
using AlfaEBetto.Data.Words;
using Godot;

namespace AlfaEBetto.Blocks;

public partial class Word : Node2D
{
	[Export]
	public PackedScene LetterBlockPackedScene { get; set; }
	[Export]
	public PackedScene NoLetterBlockPackedScene { get; set; }

	[Signal]
	public delegate void ReadyToDequeueSignalEventHandler();
	[Signal]
	public delegate void OnLetterDestructedSignalEventHandler(bool isTarget);
	[Signal]
	public delegate void OnDisableChildrenCollisionsInternalSingalEventHandler();

	public DiactricalMarkWordResource WordInfo { get; set; }

	public float CenterOffset { get; set; } = 0.0f;

	public int TargetIdx { get; set; }

	public Queue<LetterBlock> LetterBlocks { get; set; } = new();

	public LetterBlock Target { get; set; }

	private static LetterBlockBuilder _letterBuilder = null;

	private Timer _destructionTimer = new();
	public override void _Ready()
	{
		_letterBuilder ??= new LetterBlockBuilder(LetterBlockPackedScene, NoLetterBlockPackedScene);
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
		float currentX = AddNoLetterBlock();

		foreach ((char letter, int idx) in WordInfo.WithoutMark.Select((letter, idx) => (letter, idx)))
		{
			currentX = BuildLetterBlock(currentX, letter, idx);
		}

		Target.OnTargetBlockCalledDestructionSignal += Destroy;
		CenterWordPositionOn(currentX);
	}

	private void CenterWordPositionOn(float currentX)
	{
		CenterOffset = currentX / 2.0f;

		Position -= new Vector2(CenterOffset, 0);
	}

	private float AddNoLetterBlock()
	{
		NoLetterBlock noLetterBlock = _letterBuilder.BuildNoLetterBlock(
			new Vector2(0.0f, 0.0f),
			isTarget: WordInfo.HasMark is false
		);

		// always being the last to be destroyed
		noLetterBlock.OnReadyToDequeueSignal += () =>
		{
			_ = EmitSignal(nameof(ReadyToDequeueSignal));
		};

		float currentX = SetBlock(noLetterBlock, 0.0f);
		return currentX;
	}

	private float BuildLetterBlock(float currentX, char letter, int idx)
	{
		bool isTarget = WordInfo.HasMark && WordInfo.MarkIndex == idx;

		LetterBlock letterBlock = _letterBuilder.BuildLetterBlock(
			letter,
			new Vector2(currentX, 0),
			isTarget: isTarget
		);

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
