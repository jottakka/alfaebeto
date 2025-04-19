using System; // For Exception
using AlfaEBetto.Ammo;
using Godot;

namespace Alfaebeto.Components; // Corrected namespace

/// <summary>
/// A component responsible for creating and configuring instances of ammo
/// based on a provided PackedScene. Acts as a factory for AmmoBase nodes.
/// </summary>
public sealed partial class AmmoComponent : Node
{
	/// <summary>
	/// The PackedScene resource representing the ammo prefab to instantiate.
	/// Should be assigned in the Godot Inspector.
	/// </summary>
	[Export] public PackedScene PackedScene { get; set; }

	private bool _isInitialized = false;

	public override void _Ready() => Initialize();

	private void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		// Validate that the PackedScene has been assigned in the editor
		if (PackedScene == null)
		{
			GD.PrintErr($"{Name}: Exported property '{nameof(PackedScene)}' is not assigned. AmmoComponent cannot function.");
			// Deactivate or handle error as appropriate for your game
			SetProcess(false);
			SetPhysicsProcess(false);
			_isInitialized = false;
			return;
		}

		_isInitialized = true;
	}

	/// <summary>
	/// Creates, configures, and returns a new AmmoBase instance.
	/// Does NOT add the instance to the scene tree.
	/// </summary>
	/// <param name="rotationRadians">The initial rotation angle in radians.</param>
	/// <param name="globalPosition">The initial global position for the ammo.</param>
	/// <returns>A configured AmmoBase node, or null if instantiation failed.</returns>
	public AmmoBase Create(float rotationRadians, Vector2 globalPosition)
	{
		// Ensure component was initialized correctly
		if (!_isInitialized)
		{
			GD.PrintErr($"{Name}: Create called but component is not initialized (PackedScene likely missing).");
			return null;
		}

		AmmoBase ammo = null;
		try
		{
			// Instantiate the scene, checking the type
			ammo = PackedScene.Instantiate<AmmoBase>();

			if (ammo == null)
			{
				GD.PrintErr($"{Name}: Failed to instantiate scene '{PackedScene.ResourcePath}' or its root node is not {nameof(AmmoBase)}.");
				return null; // Return null if instantiation failed type check
			}

			// Configure the instantiated ammo node
			// These properties must exist on AmmoBase.cs
			ammo.ShootRadAngle = rotationRadians;
			ammo.InitialPosition = globalPosition; // AmmoBase uses this in its _Ready

			return ammo;
		}
		catch (Exception ex) // Catch potential errors during instantiation
		{
			GD.PrintErr($"{Name}: Exception during instantiation or setup of ammo scene '{PackedScene?.ResourcePath ?? "NULL"}'. Error: {ex.Message}");
			// Ensure partial instance is cleaned up if exception occurred after instantiation but before return
			ammo?.QueueFree();
			return null; // Return null on failure
		}
	}
}