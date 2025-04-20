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
	public EnemyBase Create(Vector2 globalPosition, Vector2 initialVelocity)
	{
		try
		{
			EnemyBase enemy = _enemyPackedScene.Instantiate<EnemyBase>();
			if (enemy == null)
			{
				GD.PrintErr($"{nameof(EnemyBuilder)}: Failed to instantiate scene '{_enemyPackedScene.ResourcePath}'. Root not assignable to '{nameof(EnemyBase)}'.");
				return null;
			}

			// --- *** MODIFIED LINES *** ---
			// Set the properties the EnemyBase expects in _Ready
			enemy.InitialPosition = globalPosition;
			enemy.SpawnInitialVelocity = initialVelocity;
			// Do NOT set GlobalPosition here directly if _Ready uses InitialPosition
			// --- *** END OF MODIFIED LINES *** ---

			return enemy;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{nameof(EnemyBuilder)}: Exception during instantiation: {ex.Message}");
			return null;
		}
	}
}