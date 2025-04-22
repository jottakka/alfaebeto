using System;
using Alfaebeto.Blocks;
using Alfaebeto.Components;
using Alfaebeto.Enemies.Parts;
using AlfaEBetto.Blocks;
using AlfaEBetto.Components;
using AlfaEBetto.Data.Words;
using AlfaEBetto.Enemies;
using AlfaEBetto.Enemies.Parts;
using AlfaEBetto.Extensions;
using Godot;
// Add using for EnemyAnimations if it's in a different namespace

namespace Alfaebeto.Enemies;

public abstract partial class BaseGuessEnemy : CharacterBody2D
{
	#region Exports (Common)
	[Export] public WordsSetBuilderComponent WordsSetBuilderComponent { get; protected set; }
	[Export] public TurretWing RightTurretWing { get; protected set; }
	[Export] public TurretWing LeftTurretWing { get; protected set; }
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifierUpper { get; protected set; }
	[Export] public VisibleOnScreenNotifier2D VisibleOnScreenNotifierBottom { get; protected set; }
	[Export] public AnimationPlayer AnimationPlayer { get; protected set; }
	[Export] public GemSpawnerComponent GemSpawnerComponent { get; protected set; }
	[Export] public Label GuessBlockLabel { get; protected set; }
	[Export] public float HorizontalSpeedModulus { get; protected set; } = 30.0f;
	[Export] public float VerticalVelocityModulus { get; protected set; } = 10.0f;
	[Export] public int NumberOfWrongOptions { get; protected set; } = 3;
	#endregion

	#region Signals
	[Signal]
	public delegate void OnQueueFreeSignalEventHandler();
	#endregion

	#region Properties
	public WordsSet WordSetBlocks { get; protected set; }
	#endregion

	#region Fields
	protected GuessBlockWordResource _guessBlockWordInfo;
	protected Vector2 _currentVelocity; // Renamed from _velocity to avoid conflict with Godot's property
	protected int _errorCount = 0;
	#endregion

	#region Abstract Methods (To be implemented by derived classes)
	/// <summary>
	/// Gets the specific word/article resource data for this enemy.
	/// </summary>
	protected abstract GuessBlockWordResource GetGuessResource();

	/// <summary>
	/// Builds the specific WordsSet or ArticlesSet using the WordsSetBuilderComponent.
	/// </summary>
	protected abstract WordsSet BuildBlockSetInternal(GuessBlockWordResource resource, Vector2 position, int numOptions);
	#endregion

	#region Godot Lifecycle Methods
	public override void _Ready()
	{
		// Validate required exported nodes first
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Queuing free.");
			QueueFree();
			return;
		}

		_guessBlockWordInfo = GetGuessResource();
		if (_guessBlockWordInfo == null)
		{
			GD.PrintErr($"{Name}: Failed to get guess resource. Queuing free.");
			QueueFree();
			return;
		}

		GuessBlockLabel.Text = _guessBlockWordInfo.ToBeGuessed;

		this.SetVisibilityZOrdering(VisibilityZOrdering.WordEnemy);

		// Setup Collisions (Assuming CollisionLayers is accessible)
		this.ActivateCollisionLayer(CollisionLayers.WordEnemy);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHitBox);
		this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);
		this.ActivateCollisionMask(CollisionLayers.PlayerSpecialHurtBox);
		this.ActivateCollisionMask(CollisionLayers.Player);

		// Build the specific block set via abstract method
		WordSetBlocks = BuildBlockSetInternal(_guessBlockWordInfo, Vector2.Zero, NumberOfWrongOptions);
		if (WordSetBlocks == null)
		{
			GD.PrintErr($"{Name}: Failed to build block set. Queuing free.");
			QueueFree();
			return;
		}

		AddChild(WordSetBlocks);

		// Setup state and signals AFTER block set is built and added
		SetUpInitialStates();
		SetUpSignals();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationPredelete)
		{
			// Clean up signals if necessary (though lambda connections often don't need manual disconnect unless node persists)
			EmitSignal(SignalName.OnQueueFreeSignal);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Use the built-in Velocity property for MoveAndCollide/MoveAndSlide with CharacterBody2D
		Velocity = _currentVelocity; // Set target velocity
		MoveAndCollide(Velocity * (float)delta); // Or just MoveAndSlide() if preferred
												 // After MoveAndCollide, you might want to check the result for collision details if needed.
	}
	#endregion

	#region Common Private Methods
	private void OnAnimationFinished(StringName animationName)
	{
		// Assuming EnemyAnimations class/constants are accessible
		if (animationName == EnemyAnimations.EnemyWordDeath)
		{
			// *** ADD THIS CLEANUP LOGIC HERE ***
			// Check if the specific instance being freed is a GuessBlockEnemy
			// to safely access its specific members.
			OnReadyToCleanUp();
			// *** END OF ADDED CLEANUP LOGIC ***


			// Original line that frees the enemy after the animation
			QueueFree();
		}
		// Add handling for other animations if needed (e.g., Spawn animation finish)
		// else if (animationName == ...) { ... }
	}

	protected virtual void OnReadyToCleanUp()
	{
		QueueFree();
	}

	private void SetUpInitialStates()
	{
		// Adjust turret positions based on the actual built block set's offset
		RightTurretWing.Position += new Vector2(WordSetBlocks.CenterOffset, 0);
		LeftTurretWing.Position -= new Vector2(WordSetBlocks.CenterOffset, 0);

		// Initialize velocity (used in _PhysicsProcess)
		_currentVelocity = new Vector2(
			Mathf.Abs(HorizontalSpeedModulus) * (GD.Randi() % 2 == 0 ? 1 : -1), // Simplified random direction
			Math.Abs(VerticalVelocityModulus)
		);
	}

	private void SetUpSignals()
	{
		// Screen Notifier Signals (for movement and activation)
		VisibleOnScreenNotifierUpper.ScreenEntered += OnScreenEnteredUpper;
		VisibleOnScreenNotifierUpper.ScreenExited += OnScreenExitedUpper;
		VisibleOnScreenNotifierBottom.ScreenExited += OnScreenExitedBottom;

		// Turret Wing Notifier Signals (for horizontal movement adjustment)
		// Ensure VisibleOnScreenNotifier2D is accessible within TurretWing script
		if (LeftTurretWing.VisibleOnScreenNotifier2D != null) // Safety check
		{
			LeftTurretWing.VisibleOnScreenNotifier2D.ScreenExited += OnScreenExitedLeftWing;
		}

		if (RightTurretWing.VisibleOnScreenNotifier2D != null) // Safety check
		{
			RightTurretWing.VisibleOnScreenNotifier2D.ScreenExited += OnScreenExitedRightWing;
		}

		// WordSet Signals
		WordSetBlocks.OnLetterDestructedSignal += OnLetterBlockDestruct; // Assumes signal exists
		WordSetBlocks.ReadyToDequeueSignal += OnWordSetReadyToDequeue; // Assumes signal exists

		// AnimationPlayer Signals
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
	}

	// --- Signal Handler Methods ---

	protected virtual void OnScreenEnteredUpper()
	{
		RightTurretWing?.AllowShoot();
		LeftTurretWing?.AllowShoot();
	}

	private void OnScreenExitedUpper() => _currentVelocity.Y = Mathf.Abs(VerticalVelocityModulus);

	private void OnScreenExitedBottom() => _currentVelocity.Y = -Mathf.Abs(VerticalVelocityModulus);

	private void OnScreenExitedLeftWing() => _currentVelocity.X = Mathf.Abs(HorizontalSpeedModulus);

	private void OnScreenExitedRightWing() => _currentVelocity.X = -Mathf.Abs(HorizontalSpeedModulus);

	private void OnWordSetReadyToDequeue() => AnimationPlayer?.Play(EnemyAnimations.EnemyWordDeath, customSpeed: 2);

	private void OnLetterBlockDestruct(bool isTarget)
	{
		if (isTarget)
		{
			RightTurretWing?.DisallowShoot();
			LeftTurretWing?.DisallowShoot();

			GemSpawnerComponent?.SpawnGem(
				GlobalPosition,
				GemType.Red,
				GetSpawnGemsQuantity()
			);
			AnimationPlayer?.Play(EnemyAnimations.EnemyWordDying); // Assumes animation exists
		}
		else
		{
			_errorCount++;
		}
	}

	protected virtual void DisableAttack()
	{
		RightTurretWing?.DisallowShoot();
		LeftTurretWing?.DisallowShoot();
	}

	private int GetSpawnGemsQuantity()
	{
		// Calculate percentage (ensure float division)
		float percentage = (NumberOfWrongOptions + 1 - _errorCount) / (float)(NumberOfWrongOptions + 1);

		// Use pattern matching for clarity
		return percentage switch
		{
			1.0f => 4,
			>= 0.8f => 3, // Perfect or almost perfect
			>= 0.4f => 2, // Some errors
			_ => 1       // Many errors or negative count (fallback)
		};
	}

	protected virtual bool ValidateExports()
	{

		bool overallIsValid =
			CheckNode(WordsSetBuilderComponent, nameof(WordsSetBuilderComponent)) &&
			CheckNode(RightTurretWing, nameof(RightTurretWing)) &&
			CheckNode(LeftTurretWing, nameof(LeftTurretWing)) &&
			CheckNode(VisibleOnScreenNotifierUpper, nameof(VisibleOnScreenNotifierUpper)) &&
			CheckNode(VisibleOnScreenNotifierBottom, nameof(VisibleOnScreenNotifierBottom)) &&
			CheckNode(AnimationPlayer, nameof(AnimationPlayer)) && // Use the property name here
			CheckNode(GemSpawnerComponent, nameof(GemSpawnerComponent)) &&
			CheckNode(GuessBlockLabel, nameof(GuessBlockLabel));

		return overallIsValid; // Return the overall result
	}

	protected bool CheckNode(Node node, string nodeName)
	{
		if (node == null)
		{
			GD.PrintErr($"{Name}: Exported node '{nodeName}' is not assigned or is null in the scene tree.");
			return false; // Mark as invalid if any check fails
		}
		else if (!IsInstanceValid(node))
		{
			GD.PrintErr($"{Name}: Exported node '{nodeName}' is assigned but instance is not valid (possibly freed).");
			return false;
		}

		return true;
	}

	#endregion
}