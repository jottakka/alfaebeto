using AlfaEBetto.Consts;
using Godot;

namespace AlfaEBetto.PlayerNodes; // Corrected namespace

/// <summary>
/// Processes raw player input actions each frame and provides
/// processed values (like movement direction and attack state)
/// for the Player script to use.
/// </summary>
public partial class PlayerInputProcessor : Node
{
	/// <summary>
	/// Gets the processed movement input direction vector.
	/// Length is clamped to a maximum of 1.0, suitable for velocity calculation.
	/// </summary>
	public Vector2 MovementDirection { get; private set; } = Vector2.Zero;

	/// <summary>
	/// Gets a value indicating whether the attack action is currently pressed.
	/// </summary>
	public bool IsAttacking { get; private set; }

	// No _Ready needed currently

	public override void _Process(double delta)
	{
		// Read attack state
		IsAttacking = Input.IsActionPressed(UserInput.Attack);

		// Update movement direction
		MovementDirection = GetProcessedMovementInput();
	}

	/// <summary>
	/// Reads movement actions and returns a processed direction vector
	/// clamped to a maximum length of 1.0.
	/// </summary>
	/// <returns>A Vector2 representing the input direction, with length <= 1.0.</returns>
	private Vector2 GetProcessedMovementInput()
	{
		// Method 1: Using GetActionStrength (as you currently do)
		Vector2 direction = Vector2.Zero; // Start with zero vector
		direction.X = Input.GetActionStrength(UserInput.MoveRight) - Input.GetActionStrength(UserInput.MoveLeft);
		direction.Y = Input.GetActionStrength(UserInput.MoveDown) - Input.GetActionStrength(UserInput.MoveUp);

		// Clamp the length of the resulting vector to a maximum of 1.0
		// This correctly handles analog stick diagonals and normalizes digital input.
		return direction.LimitLength(1.0f);

		/*
            // Method 2: Using Input.GetVector (Alternative if Input Map is configured)
            // Requires Input Map actions named "MoveLeft", "MoveRight", "MoveUp", "MoveDown"
            // OR a single Vector2 action named "Move".
            // return Input.GetVector(UserInput.MoveLeft, UserInput.MoveRight, UserInput.MoveUp, UserInput.MoveDown);
            // Note: Input.GetVector automatically handles normalization/clamping.
            */
	}
}