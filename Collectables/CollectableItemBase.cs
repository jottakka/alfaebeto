using AlfaEBetto;
using AlfaEBetto.CustomNodes;
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using Godot;

namespace Alfaebeto.Collectables;

public partial class CollectableItemBase : Area2D
{
	#region Exports
	[Export] public Sprite2D Sprite { get; set; }
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	[Export] public float InitialSpeed { get; set; } = 50.0f;
	[Export] public float Acceleration { get; set; } = 10.0f;
	#endregion

	#region Signals
	[Signal]
	public delegate void OnCollectedSignalEventHandler(CollectableItemBase item);
	#endregion

	#region Private Fields
	private bool _isHomingToPlayer = false; // Renamed for clarity
	private float _currentSpeed;
	private Player _cachedPlayer; // Cache the player reference
	private bool _externalSignalConnected = false; // Track external signal connection state
	#endregion

	#region Godot Methods
	public override void _Ready()
	{
		if (!ValidateExports())
		{
			GD.PrintErr($"{Name}: Missing required exported nodes. Queuing free.");
			QueueFree();
			return;
		}

		_currentSpeed = InitialSpeed;
		this.ResetCollisionLayerAndMask();
		this.ActivateCollisionLayer(CollisionLayers.Collectables);
		this.ActivateCollisionMask(CollisionLayers.Player);
		this.ActivateCollisionMask(CollisionLayers.PlayerCollectionArea);

		// Attempt to cache the player reference safely
		if (Global.Instance != null && IsInstanceValid(Global.Instance.Player))
		{
			_cachedPlayer = Global.Instance.Player;
		}
		else
		{
			GD.PrintErr($"{Name}: Could not find valid Player instance in Global singleton during _Ready.");
		}

		ConnectSignals();
	}

	public override void _ExitTree() =>
		// Ensure signals are disconnected when the node leaves the tree
		DisconnectSignals();

	public override void _PhysicsProcess(double delta)
	{
		if (!_isHomingToPlayer)
		{
			return;
		}

		if (!IsInstanceValid(_cachedPlayer))
		{
			GD.PrintRich($"[color=orange]{Name}: Player instance became invalid while homing. Stopping homing.[/color]");
			_isHomingToPlayer = false;
			return;
		}

		Vector2 directionToPlayer = GlobalPosition.DirectionTo(_cachedPlayer.GlobalPosition).Normalized();
		Vector2 velocity = directionToPlayer * _currentSpeed;
		Position += velocity * (float)delta;
		_currentSpeed += Acceleration * (float)delta;
	}
	#endregion

	#region Signal Handling
	private void ConnectSignals()
	{
		// Connect internal signals (safe to add multiple times, but usually done once)
		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;

		// Connect to player component only if player is valid, component exists, and not already connected
		if (!_externalSignalConnected && IsInstanceValid(_cachedPlayer) && _cachedPlayer.PlayerItemCollectingComponent != null)
		{
			OnCollectedSignal += _cachedPlayer.PlayerItemCollectingComponent.CollectItem;
			_externalSignalConnected = true; // Mark as connected
		}
		else if (!_externalSignalConnected && IsInstanceValid(_cachedPlayer)) // Player exists but component doesn't
		{
			GD.PrintErr($"{Name}: Player instance found, but PlayerItemCollectingComponent is null. Cannot connect collection signal.");
		}
	}

	private void DisconnectSignals()
	{
		// Disconnect internal signals (safe to remove even if not connected)
		AreaEntered -= OnAreaEntered;
		BodyEntered -= OnBodyEntered;

		// Disconnect from player component only if it was connected and instances are valid
		// Removing check for IsSignalConnected - safe to call -= even if not connected
		if (_externalSignalConnected && IsInstanceValid(_cachedPlayer) && _cachedPlayer.PlayerItemCollectingComponent != null)
		{
			OnCollectedSignal -= _cachedPlayer.PlayerItemCollectingComponent.CollectItem;
		}
		_externalSignalConnected = false; // Reset flag regardless
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body == _cachedPlayer)
		{
			EmitSignal(SignalName.OnCollectedSignal, this);
			QueueFree();
		}
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area is PlayerCollectableArea)
		{
			StartHoming();
		}
	}
	#endregion

	#region Public Methods / State Changes
	public void StartHoming()
	{
		if (_isHomingToPlayer)
		{
			return;
		}

		if (!IsInstanceValid(_cachedPlayer))
		{
			GD.PrintRich($"[color=orange]{Name}: Cannot start homing, player instance is not valid.[/color]");
			return;
		}

		_isHomingToPlayer = true;
	}
	#endregion

	#region Validation
	private bool ValidateExports()
	{
		bool overallIsValid = true;
		void CheckNode(Node node, string nodeName)
		{
			if (node == null)
			{
				GD.PrintErr($"{Name}: Exported node '{nodeName}' is not assigned.");
				overallIsValid = false;
			}
		}
		CheckNode(AnimationPlayer, nameof(AnimationPlayer));
		CheckNode(Sprite, nameof(Sprite));
		return overallIsValid;
	}
	#endregion
}