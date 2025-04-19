using System; // For Exception
using AlfaEBetto.Collectables;
using Godot;

namespace AlfaEBetto.Components;

/// <summary>
/// Spawns a random number of CollectableCoin instances at a specified position
/// within a defined scatter radius.
/// </summary>
public sealed partial class CoinSpawnerComponent : Node
{
	#region Exports
	[Export] public PackedScene CollectableCoinScene { get; set; }
	[Export(PropertyHint.Range, "1, 50, 1")] // Min 1 coin seems sensible
	public int MaxCoinsSpawn { get; set; } = 15;
	[Export(PropertyHint.Range, "1, 50, 1")]
	public int MinCoinsSpawn { get; set; } = 3;
	[Export(PropertyHint.Range, "0, 100, 1")] // Allow zero scatter
	public float CoinsScatterRadius { get; set; } = 30.0f;
	#endregion

	#region Private Fields
	private Node _cachedSceneRoot; // Cache the scene root reference
	private bool _isInitialized = false;
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// 1. Validate Export
		if (CollectableCoinScene == null)
		{
			GD.PrintErr($"{Name}: {nameof(CollectableCoinScene)} is not assigned. CoinSpawnerComponent cannot function.");
			SetProcess(false); // Deactivate if scene is missing
			SetPhysicsProcess(false);
			_isInitialized = false; // Mark as not initialized
			return;
		}

		// 2. Cache Scene Root Safely
		try
		{
			// It's generally safer to add spawned items to the current scene tree
			// rather than relying on Global.Instance.Scene unless absolutely necessary.
			// GetTree().CurrentScene is often more reliable during gameplay.
			// If this component is always part of the main playable scene, this is fine.
			// Alternatively, get a specific container node via export or GetNode.
			_cachedSceneRoot = GetTree()?.CurrentScene; // Cache the current scene root

			// Fallback if GetTree().CurrentScene is somehow null at this point
			_cachedSceneRoot ??= Global.Instance?.Scene;

			if (_cachedSceneRoot == null)
			{
				throw new Exception("Could not determine a valid scene root node to add children to.");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Failed to cache scene root. Error: {ex.Message}");
			SetProcess(false);
			SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		_isInitialized = true;
	}

	#endregion

	#region Public Methods
	/// <summary>
	/// Spawns a random number of coins scattered around the specified global position.
	/// </summary>
	/// <param name="globalPosition">The center global position for spawning coins.</param>
	public void SpawnCoins(Vector2 globalPosition)
	{
		// Ensure component is ready and scene root is valid
		if (!_isInitialized || !IsInstanceValid(_cachedSceneRoot))
		{
			GD.PrintErr($"{Name}: SpawnCoins called but component is not initialized or scene root is invalid.");
			return;
		}

		// Clamp min/max spawn values just in case they are set incorrectly in editor
		int minSpawn = Mathf.Max(0, MinCoinsSpawn); // Ensure non-negative
		int maxSpawn = Mathf.Max(minSpawn, MaxCoinsSpawn); // Ensure max >= min

		int numberOfCoins = GD.RandRange(minSpawn, maxSpawn);

		for (int i = 0; i < numberOfCoins; i++)
		{
			CollectableCoin coin = null;
			try
			{
				// Instantiate and check type
				coin = CollectableCoinScene.Instantiate<CollectableCoin>();
				if (coin == null)
				{
					GD.PrintErr($"{Name}: Failed to instantiate '{CollectableCoinScene.ResourcePath}' or its root node is not {nameof(CollectableCoin)}.");
					continue; // Skip this coin if instantiation failed
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"{Name}: Exception instantiating '{CollectableCoinScene.ResourcePath}'. Error: {ex.Message}");
				continue; // Skip this coin
			}

			// Calculate scattered position

			// Alternative: Circular Scatter
			float randomAngle = (float)GD.RandRange(0, Mathf.Tau); // Tau = 2 * PI
			float randomRadius = (float)GD.RandRange(0, CoinsScatterRadius);
			Vector2 offset = Vector2.Right.Rotated(randomAngle) * randomRadius;
			coin.GlobalPosition = globalPosition + offset;

			// Add to the cached scene root using CallDeferred for safety
			_cachedSceneRoot.CallDeferred(Node.MethodName.AddChild, coin);

			// If using an extension method `AddChildDefered`:
			// _cachedSceneRoot.AddChildDefered(coin);
		}
	}
	#endregion

	#region Private Helpers
	/// <summary>
	/// Gets a random float value within the scatter radius range.
	/// </summary>
	private float RandomScatterValue() =>
		// Use GD.RandfRange for direct float range
		(float)GD.RandRange(-(double)CoinsScatterRadius, (double)CoinsScatterRadius);
	#endregion
}