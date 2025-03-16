using Godot;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using WordProcessing.Models.DiacriticalMarks;

public sealed partial class GuessBlockEnemy : CharacterBody2D
{
	[Export]
	public WordsSetBuilderComponent WordsSetBuilderComponent { get; set; }
	[Export]
	public EnemySpawner EnemySpawnerRight { get; set; }
	[Export]
	public EnemySpawner EnemySpawnerLeft { get; set; }
	[Export]
	public TurrentWing RightTurrentWing { get; set; }
	[Export]
	public TurrentWing LeftTurrentWing { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifierUpper { get; set; }
	[Export]
	public VisibleOnScreenNotifier2D VisibleOnScreenNotifierBottom { get; set; }
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public GemSpawnerComponent GemSpawnerComponent { get; set; }
	[Export]
	public Label GuessBlockLabel { get; set; }
	[Export]
	public float HorizontalSpeedModulus { get; set; } = 30.0f;
	[Export]
	public float VerticalVelocityModulus { get; set; } = 10.0f;
	[Export]
	public int NumberOfWrongOptions { get; set; } = 2;

	[Signal]
	public delegate void OnQueueFreeSignalEventHandler();

	public WordsSet WordSetBlocks { get; set; }

	private GuessBlockWordResource _guessBlockWordInfo;

	private Vector2 _velocity;

	private int _errorCount = 0;

	public override void _Ready()
	{
		//_guessBlockWordInfo = Global.Instance.GetGetNextGuessBlockWordResource();
		_guessBlockWordInfo = HiraganaResourceProvider.GetRandomResource();

		GuessBlockLabel.Text = _guessBlockWordInfo.ToBeGuessed;

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);

		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);

		// Collision Masks to observe
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);

		BuildWordsSetBlocks();
		SetUpInitialStates();
		SetUpSignals();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			_ = EmitSignal(nameof(OnQueueFreeSignal));
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = _velocity * (float)delta;
		_ = MoveAndCollide(velocity);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		if (animationName == EnemyAnimations.EnemyWordDeath)
		{
			QueueFree();
		}
	}

	private void BuildWordsSetBlocks()
	{
		WordSetBlocks = WordsSetBuilderComponent.BuildWordsSet(_guessBlockWordInfo, new Vector2(0, 0), NumberOfWrongOptions);
		AddChild(WordSetBlocks);
	}

	private void SetUpInitialStates()
	{
		RightTurrentWing.Position += new Vector2(WordSetBlocks.CenterOffset, 0);
		LeftTurrentWing.Position -= new Vector2(WordSetBlocks.CenterOffset, 0);

		_velocity = new Vector2(
			Mathf.Abs(VerticalVelocityModulus) * Mathf.Pow(-1, GD.Randi() % 2.0f),
			Math.Abs(HorizontalSpeedModulus)
		);
	}

	private void SetUpSignals()
	{
		VisibleOnScreenNotifierUpper.ScreenEntered += () =>
		{
			EnemySpawnerLeft.AllowSpawn();
			EnemySpawnerRight.AllowSpawn();
			RightTurrentWing.AllowShoot();
			LeftTurrentWing.AllowShoot();
		};

		VisibleOnScreenNotifierUpper.ScreenExited += () =>
		{
			_velocity = new Vector2(
					_velocity.X,
					Mathf.Abs(VerticalVelocityModulus)
				);
		};

		VisibleOnScreenNotifierBottom.ScreenExited += () =>
		{
			_velocity = new Vector2(
					_velocity.X,
					-Mathf.Abs(VerticalVelocityModulus)
				);
		};

		LeftTurrentWing.VisibleOnScreenNotifier2D.ScreenExited += () =>
		{
			_velocity = new Vector2(
					Mathf.Abs(HorizontalSpeedModulus),
					_velocity.Y
				);
		};

		RightTurrentWing.VisibleOnScreenNotifier2D.ScreenExited += () =>
		{
			_velocity = new Vector2(
					-Mathf.Abs(HorizontalSpeedModulus),
					_velocity.Y
				);
		};

		WordSetBlocks.OnLetterDestructedSignal += OnLetterBlockDestruct;

		WordSetBlocks.ReadyToDequeueSignal += () =>
		{
			AnimationPlayer.Play(EnemyAnimations.EnemyWordDeath, customSpeed: 2);

		};

		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	private void OnLetterBlockDestruct(bool isTarget)
	{
		if (isTarget)
		{
			EnemySpawnerLeft.DesallowSpawn();
			EnemySpawnerRight.DesallowSpawn();
			RightTurrentWing.DesallowShoot();
			LeftTurrentWing.DesallowShoot();
			GemSpawnerComponent.SpawnGem(
				GlobalPosition,
				GemType.Red,
				GetSpawnGemsQuantity()
			);
			AnimationPlayer.Play(EnemyAnimations.EnemyWordDying);
		}
		else
		{
			_errorCount++;
		}
	}

	private int GetSpawnGemsQuantity()
	{
		float percentage =
			(NumberOfWrongOptions + 1 - _errorCount) / (float)(NumberOfWrongOptions+1);

		return percentage switch
		{
			1.0f => 4,
			>= 0.8f => 3,
			>= 0.4f => 2,
			_ => 1
		};
	}
}

public static class HiraganaResourceProvider
{
	// Holds the entire set of GuessBlockWordResource for Hiragana → Romaji
	public static GuessBlockWordResource[] AllHiraganaToRomaji { get; }

	// Static constructor: builds the resource array once at class load
	static HiraganaResourceProvider()
	{
		// All 46 basic hiragana
		var hiraganaToRomaji = new (string Hiragana, string Romaji)[]
		{
			("あ","a"),  ("い","i"),  ("う","u"),  ("え","e"),  ("お","o"),
			("か","ka"), ("き","ki"), ("く","ku"), ("け","ke"), ("こ","ko"),
			("さ","sa"), ("し","shi"),("す","su"), ("せ","se"), ("そ","so"),
			("た","ta"), ("ち","chi"),("つ","tsu"),("て","te"), ("と","to"),
			("な","na"), ("に","ni"), ("ぬ","nu"), ("ね","ne"), ("の","no"),
			("は","ha"), ("ひ","hi"), ("ふ","fu"), ("へ","he"), ("ほ","ho"),
			("ま","ma"), ("み","mi"), ("む","mu"), ("め","me"), ("も","mo"),
			("や","ya"), ("ゆ","yu"), ("よ","yo"),
			("ら","ra"), ("り","ri"), ("る","ru"), ("れ","re"), ("ろ","ro"),
			("わ","wa"), ("を","wo"),
			("ん","n")
		};

		AllHiraganaToRomaji = hiraganaToRomaji
			.Select(pair => new GuessBlockWordResource
			{
				DiactricalMarkSubCategoryType = GuessBlockRuleType.GuessRomangiFromHiragana,
				ToBeGuessed = pair.Hiragana,
				Answer = pair.Romaji
			})
			.ToArray();
	}

	// Returns a random index (0 to AllHiraganaToRomaji.Length - 1)
	public static int GetRandomResourceIndex()
	{
		return GD.RandRange(0, AllHiraganaToRomaji.Length - 1);
	}

	// Returns a random GuessBlockWordResource from the array
	public static GuessBlockWordResource GetRandomResource()
	{
		int index = GetRandomResourceIndex();
		return AllHiraganaToRomaji[index];
	}
}
