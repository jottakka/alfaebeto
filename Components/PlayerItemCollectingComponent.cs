using Godot;
public sealed partial class PlayerItemCollectingComponent : Node
{
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
			case CollectableCoin:
				break;
			default:
				GD.PrintErr("Item type not recognized");
				break;
		}
	}

	private void CollectHealthItem(CollectableHealthItem healthItem)
	{
		_player.HealthComponent.Heal(healthItem.HealingPoints);
	}

	private void CollectShieldItem(CollectableShieldItem shieldItem)
	{
		_player.PlayerShield.AddShieldPoints(shieldItem.ShieldPoints);
	}
}
