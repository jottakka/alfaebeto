using Alfaebeto.Collectables; // Assuming Collectable types and GemType are here
using AlfaEBetto.Collectables;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.Components; // Corrected namespace

/// <summary>
/// Attached to the Player node, this component handles the logic for applying
/// effects when different CollectableItemBase types are collected.
/// </summary>
public sealed partial class PlayerItemCollectingComponent : Node
{
	#region Exports
	/// <summary>
	/// Audio player for the coin collection sound effect. Assign in Inspector.
	/// </summary>
	[Export] public AudioStreamPlayer CoinAudioStreamPlayer { get; set; }

	/// <summary>
	/// Audio player for the gem collection sound effect. Assign in Inspector.
	/// </summary>
	[Export] public AudioStreamPlayer GemAudioStreamPlayer { get; set; }
	#endregion

	#region Private Fields
	private Player _cachedPlayer; // Cache the reference to the parent Player node
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

		// 1. Validate Exports
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Component may not play sounds.");
			// Decide if this is critical enough to deactivate
			// SetProcess(false); SetPhysicsProcess(false);
			// _isInitialized = false; // Mark as partially failed?
			// return;
		}

		// 2. Get and Cache Player Reference
		_cachedPlayer = GetParent<Player>();
		if (_cachedPlayer == null)
		{
			GD.PrintErr($"{Name}: Parent node is not or does not inherit from Player. Component cannot function.");
			SetProcess(false);
			SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		// 3. Validate Player Components (Optional but recommended)
		if (_cachedPlayer.HealthComponent == null)
		{
			GD.PushWarning($"{Name}: Parent Player is missing HealthComponent.");
		}

		if (_cachedPlayer.PlayerShield == null)
		{
			GD.PushWarning($"{Name}: Parent Player is missing PlayerShield.");
		}
		// Add checks for AddMoney, AddGem methods/properties if needed

		_isInitialized = true;
		// GD.Print($"{Name}: Initialized.");
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// Public method called (usually via signal connection from CollectableItemBase)
	/// when an item should be collected by the player.
	/// </summary>
	/// <param name="item">The CollectableItemBase instance that was collected.</param>
	public void CollectItem(CollectableItemBase item)
	{
		// Ensure component is ready and item is valid
		if (!_isInitialized || !IsInstanceValid(item))
		{
			GD.PrintErr($"{Name}: CollectItem called but component not initialized or item is invalid.");
			return;
		}

		// Use pattern matching on the item type
		switch (item)
		{
			case CollectableShieldItem shieldItem:
				CollectShieldItem(shieldItem); // Renamed internal method
				break;
			case CollectableHealthItem healthItem:
				CollectHealthItem(healthItem); // Renamed internal method
				break;
			case CollectableCoin coin:
				CollectCoinItem(coin); // Renamed internal method
				break;
			case CollectableGem gem:
				CollectGemItem(gem); // Renamed internal method
				break;
			default:
				GD.PrintErr($"{Name}: Collected item type not recognized: {item.GetType().Name}");
				break;
		}

		// Note: The collected item usually calls QueueFree() on itself after emitting the signal
	}
	#endregion

	#region Private Collection Logic Helpers

	private void CollectHealthItem(CollectableHealthItem healthItem) =>
		// Use cached player reference with null check on component
		_cachedPlayer?.HealthComponent?.Heal(healthItem.HealingPoints);// GD.Print($"Collected Health: {healthItem.HealingPoints}");// Add sound effect if needed

	private void CollectShieldItem(CollectableShieldItem shieldItem) =>
		// Use cached player reference with null check on component
		_cachedPlayer?.PlayerShield?.AddShieldPoints(shieldItem.ShieldPoints);// GD.Print($"Collected Shield: {shieldItem.ShieldPoints}");// Add sound effect if needed

	private void CollectCoinItem(CollectableCoin coin)
	{
		// Check cached player first
		if (_cachedPlayer == null)
		{
			return;
		}

		// Assuming AddMoney exists directly on Player script
		_cachedPlayer.AddMoney(coin.Value);

		// Play sound safely
		CoinAudioStreamPlayer?.Play();
		// GD.Print($"Collected Coin: {coin.Value}");
	}

	private void CollectGemItem(CollectableGem gem)
	{
		// Check cached player first
		if (_cachedPlayer == null)
		{
			return;
		}

		// Assuming AddGem exists directly on Player script
		_cachedPlayer.AddGem(gem.GemType);

		// Play sound safely
		GemAudioStreamPlayer?.Play();
		// GD.Print($"Collected Gem: {gem.GemType}");
	}

	#endregion

	#region Validation
	/// <summary>
	/// Validates that essential exported nodes are assigned.
	/// </summary>
	private bool ValidateExports()
	{
		bool isValid = true;
		// Check audio players - log warning if missing, as it might not be critical
		if (CoinAudioStreamPlayer == null)
		{
			GD.PushWarning($"{Name}: Exported node '{nameof(CoinAudioStreamPlayer)}' is not assigned. Coin sounds will not play.");
			// isValid = false; // Decide if this should prevent initialization
		}

		if (GemAudioStreamPlayer == null)
		{
			GD.PushWarning($"{Name}: Exported node '{nameof(GemAudioStreamPlayer)}' is not assigned. Gem sounds will not play.");
			// isValid = false; // Decide if this should prevent initialization
		}

		return isValid; // Currently only warns, doesn't fail initialization
	}
	#endregion
}