using System; // For Exception
using System.Linq; // For Any()
using Alfaebeto.Collectables; // Assuming CollectableItemBase is here
using Godot;
// using AlfaEBetto.PlayerNodes; // Not currently used

namespace AlfaEBetto.Components; // Corrected namespace

/// <summary>
/// Spawns a random CollectableItemBase from a list of configured scenes
/// at a specified global position.
/// </summary>
public sealed partial class RandomItemDropComponent : Node
{
	#region Exports
	/// <summary>
	/// An array of PackedScene resources, each representing a collectable item prefab.
	/// One of these will be chosen randomly to spawn. Assign in Inspector.
	/// </summary>
	[Export] public PackedScene[] CollectableItemScenes { get; set; } = Array.Empty<PackedScene>(); // Initialize to empty array
	#endregion

	#region Private Fields
	private Node _cachedSceneRoot; // Cache the scene root reference
	private bool _isInitialized = false;
	// private Player Player => Global.Instance.Player; // Unused, removed for now
	#endregion

	#region Godot Methods
	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// 1. Validate Exported Array
		if (CollectableItemScenes == null || !CollectableItemScenes.Any())
		{
			GD.PrintRich($"[color=orange]{Name}: {nameof(CollectableItemScenes)} array is null or empty. This component will not drop any items.[/color]");
			// We don't necessarily need to deactivate, it just won't do anything.
			// SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false; // Mark as not ready to spawn
			return;
		}
		// Optional: Validate that scenes within the array are not null
		for (int i = 0; i < CollectableItemScenes.Length; i++)
		{
			if (CollectableItemScenes[i] == null)
			{
				GD.PrintErr($"{Name}: Found null PackedScene at index {i} in {nameof(CollectableItemScenes)}. It will be skipped during drops.");
				// We can allow it to continue, just skipping null entries later.
			}
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
			SetProcess(false); SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		_isInitialized = true;
		// GD.Print($"{Name}: Initialized with {CollectableItemScenes.Length} potential items.");
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Instantiates and adds a randomly chosen collectable item from the
	/// CollectableItemScenes array to the current scene at the specified position.
	/// </summary>
	/// <param name="globalPosition">The global position where the item should appear.</param>
	public void DropRandomItem(Vector2 globalPosition)
	{
		// Ensure component is ready, scene root is valid, and there are items to drop
		if (!_isInitialized || !IsInstanceValid(_cachedSceneRoot) || CollectableItemScenes == null || CollectableItemScenes.Length == 0)
		{
			// Error logged during Initialize if scenes array is bad
			// GD.PrintErr($"{Name}: DropRandomItem called but component is not ready or has no scenes configured.");
			return;
		}

		// Select a random scene (ensure index is valid)
		int itemIdx = GD.RandRange(0, CollectableItemScenes.Length - 1);
		PackedScene itemPackedScene = CollectableItemScenes[itemIdx];

		// Check if the selected scene is null (if allowed during init validation)
		if (itemPackedScene == null)
		{
			GD.PushWarning($"{Name}: Selected item scene at index {itemIdx} is null. Skipping drop.");
			return;
		}

		// Instantiate and configure
		CollectableItemBase item = null;
		try
		{
			item = itemPackedScene.Instantiate<CollectableItemBase>();
			if (item == null)
			{
				GD.PrintErr($"{Name}: Failed to instantiate '{itemPackedScene.ResourcePath}' or its root node is not {nameof(CollectableItemBase)}.");
				return; // Cannot proceed if instantiation failed
			}

			item.GlobalPosition = globalPosition;

			// Add to the scene using deferred call
			_cachedSceneRoot.CallDeferred(Node.MethodName.AddChild, item);
			// _cachedSceneRoot.AddChildDefered(item); // If using extension
		}
		catch (Exception ex)
		{
			GD.PrintErr($"{Name}: Exception during instantiation or setup of item scene '{itemPackedScene?.ResourcePath ?? "NULL"}'. Error: {ex.Message}");
			item?.QueueFree(); // Clean up potential partial instance
		}
	}
	#endregion
}