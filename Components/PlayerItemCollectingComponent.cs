using AlfaEBetto.Collectables;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace AlfaEBetto.Components;

public sealed partial class PlayerItemCollectingComponent : Node
{
	[Export]
	public AudioStreamPlayer CoinAudioStreamPlayer { get; set; }
	[Export]
	public AudioStreamPlayer GemAudioStreamPlayer { get; set; }

	private Player _player => GetParent<Player>();

	public void CollectItem(CollectableItemBase item)
	{
		switch (item)
		{
			case CollectableShieldItem shieldItem:
				CollectShieldItem(shieldItem);
				break;
			case CollectableHealthItem healthItem:
				CollectHealthItem(healthItem);
				break;
			case CollectableCoin coin:
				CollectCoin(coin);
				break;
			case CollectableGem gem:
				CollectGem(gem.GemType);
				break;
			default:
				GD.PrintErr($"Item type not recognized {item.GetType().Name}");
				break;
		}
	}

	private void CollectHealthItem(CollectableHealthItem healthItem) => _player.HealthComponent.Heal(healthItem.HealingPoints);

	private void CollectShieldItem(CollectableShieldItem shieldItem) => _player.PlayerShield.AddShieldPoints(shieldItem.ShieldPoints);

	private void CollectCoin(CollectableCoin coin)
	{
		_player.AddMoney(coin.Value);
		CoinAudioStreamPlayer.Play();
	}

	private void CollectGem(GemType gemType)
	{
		_player.AddGem(gemType);
		GemAudioStreamPlayer.Play();
	}
}
