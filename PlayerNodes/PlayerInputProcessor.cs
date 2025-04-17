using AlfaEBetto.Consts;
using Godot;

namespace AlfaEBetto.PlayerNodes
{
	public partial class PlayerInputProcessor : Node
	{
		public Vector2 MovementDirection { get; private set; } = Vector2.Zero;

		public bool IsAttacking { get; private set; }

		public override void _Process(double delta)
		{
			IsAttacking = Input.IsActionPressed(UserInput.Attack);
			MovementDirection = GetNewMovementDirection();
		}

		private Vector2 GetNewMovementDirection()
		{
			Vector2 direction = new()
			{
				X = Input.GetActionStrength(UserInput.MoveRight) - Input.GetActionStrength(UserInput.MoveLeft),
				Y = Input.GetActionStrength(UserInput.MoveDown) - Input.GetActionStrength(UserInput.MoveUp)
			};

			// If input is digital, normalize it for diagonal movement
			if (Mathf.Abs(direction.X) == 1 && Mathf.Abs(direction.Y) == 1)
			{
				direction = direction.Normalized();
			}

			return direction;
		}
	}
}
