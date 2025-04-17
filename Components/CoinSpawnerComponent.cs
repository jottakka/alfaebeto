using AlfaEBetto.Collectables;
using AlfaEBetto.Extensions;
using Godot;

namespace AlfaEBetto.Components
{
	public sealed partial class CoinSpawnerComponent : Node
	{
		[Export]
		public PackedScene CollectableCoinScene { get; set; }
		[Export]
		public int MaxCoinsSpawn { get; set; } = 15;
		[Export]
		public int MinCoinsSpawn { get; set; } = 3;
		[Export]
		public float CoinsScatterRadius { get; set; } = 30.0f;

		private Node _scene => Global.Instance.Scene;

		public override void _Ready()
		{
		}

		public void SpawnCoins(Vector2 position)
		{
			int numberOfCoins = GD.RandRange(MinCoinsSpawn, MaxCoinsSpawn);
			for (int i = 0; i < numberOfCoins; i++)
			{
				CollectableCoin coin = CollectableCoinScene.Instantiate<CollectableCoin>();
				coin.GlobalPosition = new Vector2(
					position.X + RandomScatterRange(),
					position.Y + RandomScatterRange()
				);

				_scene.AddChildDeffered(coin);
			}
		}

		private float RandomScatterRange()
		{
			return (float)GD.RandRange(
				-(double)CoinsScatterRadius,
				(double)CoinsScatterRadius);
		}
	}
}
