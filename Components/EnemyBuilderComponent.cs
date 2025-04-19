using System; // For Exception and ArgumentNullException
using AlfaEBetto.Enemies;
using Godot;

namespace AlfaEBetto.Components; // Assuming builders reside alongside components

/// <summary>
/// Responsible for instantiating EnemyBase scenes.
/// Sets the initial position and allows the enemy's internal _Ready method
/// to handle specific setup like initial velocity.
/// </summary>
public sealed class EnemyBuilder
{
	private readonly PackedScene _enemyPackedScene;

	/// <summary>
	/// Creates a new EnemyBuilder.
	/// </summary>
	/// <param name="enemyPackedScene">The PackedScene resource for the EnemyBase scene. Must not be null.</param>
	/// <exception cref="ArgumentNullException">Thrown if enemyPackedScene is null.</exception>
	public EnemyBuilder(PackedScene enemyPackedScene) =>
		// Validate the PackedScene immediately on construction
		_enemyPackedScene = enemyPackedScene ?? throw new ArgumentNullException(nameof(enemyPackedScene), "Enemy PackedScene cannot be null.");

	/// <summary>
	/// Creates a new EnemyBase instance and sets its initial global position.
	/// Does NOT add the instance to the scene tree.
	/// </summary>
	/// <param name="globalPosition">The initial global position for the enemy node.</param>
	/// <param name="initialVelocity">
	/// (Note: This parameter is currently unused as velocity should typically
	/// be handled within the enemy's own _Ready or state logic).
	/// Included for potential future use or adaptation if EnemyBase provides a setter method.
	/// </param>
	/// <returns>The instantiated EnemyBase node, or null if instantiation fails.</returns>
	public EnemyBase Create(Vector2 globalPosition, Vector2 initialVelocity) // Keep velocity param for now, but don't use directly
	{
		try
		{
			// Instantiate the scene using the generic method for type safety
			EnemyBase enemy = _enemyPackedScene.Instantiate<EnemyBase>();

			// Check if instantiation actually returned the expected type
			if (enemy == null)
			{
				GD.PrintErr($"{nameof(EnemyBuilder)}: Failed to instantiate scene '{_enemyPackedScene.ResourcePath}'. The root node is not assignable to '{nameof(EnemyBase)}'.");
				return null;
			}

			// Configure the instantiated node
			enemy.GlobalPosition = globalPosition;

			return enemy;
		}
		catch (Exception ex) // Catch potential errors during instantiation
		{
			GD.PrintErr($"{nameof(EnemyBuilder)}: Exception during instantiation or setup of enemy scene '{_enemyPackedScene?.ResourcePath ?? "NULL"}'. Error: {ex.Message}");
			// Note: Cannot QueueFree potential 'enemy' instance here easily if exception
			// occurs after instantiation but before return, as it's not added to tree yet.
			// Godot's GC should handle it eventually if not referenced elsewhere.
			return null; // Return null on failure
		}
	}
}