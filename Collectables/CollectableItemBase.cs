using Godot;


public partial class CollectableItemBase : Area2D
{
    [Export]
    public CollectableItemResource CollectableItemResource { get; set; }
    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }
    [Export]
    public Sprite2D Sprite { get; set; }
    [Export]
    public float InitialSpeed { get; set; } = 50.0f;
    [Export]
    public float Acceleration { get; set; } = 10.0f;

    [Signal]
    public delegate void OnCollectedSignalEventHandler();

    private bool _IsBeingCollected = false;
    private Player _player => Global.Instance.Player;
    private float _speed;

    public override void _Ready()
    {
        _speed = InitialSpeed;
        //if (CollectableItemResource.Texture is not null)
        //{
        //	Sprite.Texture = CollectableItemResource.Texture;
        //}
        //else
        //
        //	var scene = CollectableItemResource.Scene.Instantiate();
        //	AddChild(scene);
        //}
        this.ResetCollisionLayerAndMask();

        this.ActivateCollisionLayer(CollisionLayers.Collectables);

        this.ActivateCollisionMask(CollisionLayers.Player);
        this.ActivateCollisionMask(CollisionLayers.PlayerCollectionArea);

        // when the collectable item enters the player collection area
        AreaEntered += OnAreaShapeEntered;
        // when it reachs the player itself
        BodyEntered += OnPlayerBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_IsBeingCollected is true)
        {
            var directionToPlayer = GlobalPosition.DirectionTo(_player.GlobalPosition);
            var velocity = directionToPlayer * _speed;
            Position += velocity * (float)delta;
            _speed += Acceleration;
        }
    }

    private void OnPlayerBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            EmitSignal(nameof(OnCollectedSignal));
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

