using System; // Potentially needed for other operations
using AlfaEBetto.Extensions;
using AlfaEBetto.PlayerNodes;
using AlfaEBetto.Weapons;
using Godot;

namespace AlfaEBetto.Components
{
	// Assuming these classes/components exist:
	// using YourProject.Global; // Global, StageBase
	// using YourProject.Actors; // Player, Laser
	// using YourProject.Input; // PlayerInputProcessor
	// using YourProject.Extensions; // AddChildDeffered (if it's an extension method)

	public sealed partial class WeaponComponent : Node
	{
		// --- Exports ---
		[Export] public PackedScene LaserPackedScene { get; set; }
		[Export] public PlayerInputProcessor PlayerInputProcessor { get; set; }
		[Export] public Godot.Timer CooldownTimer { get; set; }
		[Export] public AudioStreamPlayer2D LaserSound { get; set; }

		// --- Properties ---
		// Access Global properties safely
		private Player _player => Global.Instance?.Player;
		private Node _scene => Global.Instance?.Scene; // Assuming Scene holds the Node to add laser to

		// --- State ---
		private bool _isWaitingCooldown = false;

		// --- Godot Methods ---

		public override void _Ready()
		{
			if (!ValidateExports())
			{
				GD.PrintErr($"{Name}: Missing required exported nodes. Deactivating weapon.");
				SetProcess(false); // Disable _Process if setup fails
				return;
			}

			// Configure Audio Player
			// Consider making MaxPolyphony an export if needed
			LaserSound.MaxPolyphony = 5;

			// Connect timer signal
			CooldownTimer.Timeout += OnCooldownTimeout;
		}

		public override void _ExitTree()
		{
			// Disconnect timer signal when removed from tree
			if (IsInstanceValid(CooldownTimer))
			{
				CooldownTimer.Timeout -= OnCooldownTimeout;
			}
		}

		public override void _Process(double delta)
		{
			// Check dependencies and state before attempting to shoot
			if (_isWaitingCooldown || !IsInstanceValid(PlayerInputProcessor) || !PlayerInputProcessor.IsAttacking)
			{
				return;
			}

			// If attack input is active and not waiting for cooldown, shoot.
			ShootLaser();
		}

		// --- Private Methods ---

		/// <summary>
		/// Handles spawning the laser projectile.
		/// </summary>
		private void ShootLaser()
		{
			// --- Validate required nodes/resources before proceeding ---
			Player currentPlayer = _player; // Cache for validity checks
			Node currentScene = _scene;     // Cache for validity checks

			if (!IsInstanceValid(currentPlayer))
			{
				GD.PrintErr($"{Name}: Cannot shoot, Player instance is invalid.");
				return;
			}
			// Assuming Player has a MuzzlePosition Node2D property/export
			if (!IsInstanceValid(currentPlayer.MuzzlePosition))
			{
				GD.PrintErr($"{Name}: Cannot shoot, Player MuzzlePosition is invalid.");
				return;
			}

			if (!IsInstanceValid(currentScene))
			{
				GD.PrintErr($"{Name}: Cannot shoot, Scene instance is invalid.");
				return;
			}

			if (LaserPackedScene == null)
			{
				GD.PrintErr($"{Name}: Cannot shoot, LaserPackedScene is not set.");
				return;
			}
			// -------------------------------------------------------------

			// Set cooldown flag immediately
			_isWaitingCooldown = true;

			// Instantiate Laser
			Laser laser = null;
			try
			{
				laser = LaserPackedScene.Instantiate<Laser>();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"{Name}: Failed to instantiate LaserPackedScene. Error: {ex.Message}");
				_isWaitingCooldown = false; // Reset cooldown if instantiation failed
				return;
			}

			if (!IsInstanceValid(laser)) // Check if instantiation succeeded
			{
				GD.PrintErr($"{Name}: Failed to instantiate LaserPackedScene (returned null or invalid).");
				_isWaitingCooldown = false; // Reset cooldown
				return;
			}

			// Position and add the laser
			laser.GlobalPosition = currentPlayer.MuzzlePosition.GlobalPosition; // Use GlobalPosition for world space
																				// Assuming AddChildDeffered is an extension method on Node
			currentScene.AddChildDeffered(laser);

			// Play sound and start timer (use null-conditional for safety)
			LaserSound?.Play();
			CooldownTimer?.Start();
		}

		/// <summary>
		/// Resets the cooldown flag when the timer times out.
		/// </summary>
		private void OnCooldownTimeout() => _isWaitingCooldown = false;

		/// <summary>
		/// Validates that essential exported nodes/resources are assigned.
		/// </summary>
		private bool ValidateExports()
		{
			bool isValid = true;
			if (LaserPackedScene == null) { GD.PrintErr($"{Name}: Missing LaserPackedScene!"); isValid = false; }

			if (PlayerInputProcessor == null) { GD.PrintErr($"{Name}: Missing PlayerInputProcessor!"); isValid = false; }

			if (CooldownTimer == null) { GD.PrintErr($"{Name}: Missing CooldownTimer!"); isValid = false; }

			if (LaserSound == null) { GD.PrintErr($"{Name}: Missing LaserSound!"); isValid = false; }

			return isValid;
		}
	}
}
