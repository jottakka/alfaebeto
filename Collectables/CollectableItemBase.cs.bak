using Godot;

public partial class CollectableItemBase : Area2D
{
	[Export]
	public AnimationPlayer AnimationPlayer { get; set; }
	[Export]
	public Sprite2D Sprite { get; set; }
	[Export]
	public float InitialSpeed { get; set; } = 50.0f;
	[Export]
	public float Acceleration { get; set; } = 10.0f;

	[Signal]
	public delegate void OnCollectedSignalEventHandler(CollectableItemBase item);

	private bool _IsBeingCollected = false;
	private Player _player => Global.Instance.Player;
	private float _speed;

	public override void _Ready()
	{
		_speed = InitialSpeed;
		this.ResetCollisionLayerAndMask();

		this.ActivateCollisionLayer(CollisionLayers.Collectables);

		this.ActivateCollisionMask(CollisionLayers.Player);
		this.ActivateCollisionMask(CollisionLayers.PlayerCollectionArea);

		OnCollectedSignal += _player.PlayerItemCollectingComponent.CollectItem;

		// when the collectable item enters the player collection area
		AreaEntered += OnAreaShapeEntered;
		// when it reachs the player itself
		BodyEntered += OnPlayerBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_IsBeingCollected is true)
		{
			Vector2 directionToPlayer = GlobalPosition.DirectionTo(_player.GlobalPosition);
			Vector2 velocity = directionToPlayer * _speed;
			Position += velocity * (float)delta;
			_speed += Acceleration;
		}
	}

	private void OnPlayerBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			_ = EmitSignal(nameof(OnCollectedSignal), this);

			QueueFree();
		}
	}

	private void OnAreaShapeEntered(Area2D area)
	{
		if (area is PlayerCollectableArea)
		{
			_IsBeingCollected = true;
		}
	}
}

