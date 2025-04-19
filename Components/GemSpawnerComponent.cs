using System; // For Exception
using AlfaEBetto.Collectables;
using Godot;

namespace AlfaEBetto.Components; // Corrected namespace

/// <summary>
/// Spawns CollectableGem instances at a specified position.
/// Can spawn multiple gems in a circular pattern around the position.
/// </summary>
public sealed partial class GemSpawnerComponent : Node
{
	#region Exports
	[Export] public PackedScene CollectableGemScene { get; set; }
	[Export(PropertyHint.Range, "0, 100, 1")] // Renamed from SpawnRadium
	public float SpawnRadius { get; set; } = 30.0f;
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
		if (CollectableGemScene == null)
		{
			GD.PrintErr($"{Name}: {nameof(CollectableGemScene)} is not assigned. GemSpawnerComponent cannot function.");
			SetProcess(false); // Deactivate if scene is missing
			SetPhysicsProcess(false);
			_isInitialized = false; // Mark as not initialized
			return;
		}

		// 2. Cache Scene Root Safely
		try
		{
			_cachedSceneRoot = GetTree()?.CurrentScene;
			_cachedSceneRoot ??= Global.Instance?.Scene; // Fallback
			if (_cachedSceneRoot == null) { throw new Exception("Could not determine a valid scene root node."); }
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
	/// Spawns a specified quantity of gems of a given type at a position.
	/// If quantity is 1, spawns at the exact position.
	/// If quantity > 1, spawns in a circular pattern around the position.
	/// </summary>
	/// <param name="globalPosition">The center global position for spawning gems.</param>
	/// <param name="gemType">The type of gem to spawn.</param>
	/// <param name="quantity">The number of gems to spawn (defaults to 1).</param>
	public void SpawnGem(Vector2 globalPosition, GemType gemType, int quantity = 1)
	{
		// Ensure component is ready and scene root is valid
		if (!_isInitialized || !IsInstanceValid(_cachedSceneRoot))
		{
			GD.PrintErr($"{Name}: SpawnGem called but component is not initialized or scene root is invalid.");
			return;
		}

		if (quantity <= 0)
		{
			return; // Don't spawn if quantity is zero or negative
		}

		// --- Spawning Logic ---
		// If quantity is 1, spawn directly at the position
		if (quantity == 1)
		{
			SpawnSingleGem(globalPosition, gemType);
		}
		// If quantity > 1, spawn in a circle
		else
		{
			SpawnGemsInCircle(globalPosition, gemType, quantity);
		}
	}
	#endregion

	#region Private Spawn Helpers
	private void SpawnSingleGem(Vector2 globalPosition, GemType gemType)
	{
		CollectableGem gem = InstantiateGem(gemType);
		if (gem == null)
		{
			return; // Instantiation failed
		}

		gem.GlobalPosition = globalPosition;
		_cachedSceneRoot.CallDeferred(Node.MethodName.AddChild, gem);
		// _cachedSceneRoot.AddChildDefered(gem); // If using extension
	}

	private void SpawnGemsInCircle(Vector2 centerPosition, GemType gemType, int quantity)
	{
		// Get a random starting direction for the circle spread
		Vector2 startDirection = GetRandomNormalizedDirection2D();
		// Calculate the angle step between each gem
		float angleStep = Mathf.Tau / quantity; // Tau = 2 * PI

		for (int i = 0; i < quantity; i++)
		{
			CollectableGem gem = InstantiateGem(gemType);
			if (gem == null)
			{
				continue; // Skip if instantiation failed
			}

			// Calculate offset: Rotate the starting direction by the angle step * index
			Vector2 offset = startDirection.Rotated(angleStep * i) * SpawnRadius;
			gem.GlobalPosition = centerPosition + offset;

			_cachedSceneRoot.CallDeferred(Node.MethodName.AddChild, gem);
			// _cachedSceneRoot.AddChildDefered(gem); // If using extension
		}
	}

	/// <summary>
	/// Instantiates a CollectableGem scene and performs basic setup.
	/// </summary>
	/// <returns>The instantiated gem node or null on failure.</returns>
	private CollectableGem InstantiateGem(GemType gemType)
	{
		CollectableGem gem = null;
		try
		{
			gem = CollectableGemScene.Instantiate<CollectableGem>();
			if (gem == null)
			{
				GD.PrintErr($"{Name}: Failed to instantiate '{CollectableGemScene.ResourcePath}' or root node is not {nameof(CollectableGem)}.");
				return null;
			}

			gem.GemType = gemType; // Set type AFTER instantiation
			return gem;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Exception instantiating '{CollectableGemScene?.ResourcePath ?? "NULL"}'. Error: {ex.Message}");
			gem?.QueueFree(); // Clean up potential partial instance if exception happened after instantiate but before return
			return null;
		}
	}

	/// <summary>
	/// Gets a random normalized 2D direction vector.
	/// </summary>
	private Vector2 GetRandomNormalizedDirection2D()
	{
		float angle = GD.Randf() * Mathf.Tau; // Use GD.Randf() for 0.0 to 1.0 range
											  // Return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); // No need to normalize if calculated this way
		return Vector2.Right.Rotated(angle); // Simpler way to get unit vector at random angle
	}
	#endregion
}