using Godot;
public sealed partial class PlayerItemCollectingComponent : Node
{
	[Export]
	public AudioStreamPlayer AudioStreamPlayer { get; set; }

	private Player _player => GetParent<Player>();
	private Timer _timer = new();
	private int _coinsCount = 0;

	public override void _Ready()
	{
		AddChild(_timer);
		_timer.Timeout += () =>
		{
			AudioStreamPlayer.Play();
			_coinsCount--;

			if (_coinsCount == 0)
			{
				_timer.Stop();
			}
		};
	}

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
			default:
				GD.PrintErr($"Item type not recognized {item.GetType().Name}");
				break;
		}
	}

	private void AddCoinCount()
	{
		if (_coinsCount == 0)
		{
			AudioStreamPlayer.Play();
			_timer.Start(0.03);
		}

		_coinsCount++;
	}

	private void CollectHealthItem(CollectableHealthItem healthItem)
	{
		_player.HealthComponent.Heal(healthItem.HealingPoints);
	}

	private void CollectShieldItem(CollectableShieldItem shieldItem)
	{
		_player.PlayerShield.AddShieldPoints(shieldItem.ShieldPoints);
	}

	private void CollectCoin(CollectableCoin coin)
	{
		_player.AddMoney(coin.Value);

		AddCoinCount();
	}
}
