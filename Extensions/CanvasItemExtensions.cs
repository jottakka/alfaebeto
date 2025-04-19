using Godot;
// Assuming VisibilityZOrdering enum is defined elsewhere, e.g.:
// using Alfaebeto.Consts;

namespace AlfaEBetto.Extensions; // Corrected namespace

/// <summary>
/// Provides extension methods for Godot's CanvasItem class.
/// </summary>
public static class CanvasItemExtensions
{
	/// <summary>
	/// Sets the ZIndex of the CanvasItem based on a VisibilityZOrdering enum value.
	/// Ensures ZIndex values are managed consistently using predefined enum constants.
	/// </summary>
	/// <param name="canvasItem">The CanvasItem to modify.</param>
	/// <param name="zOrdering">The desired visibility Z-ordering layer (enum value).</param>
	public static void SetVisibilityZOrdering(this CanvasItem canvasItem, VisibilityZOrdering zOrdering)
	{
		// Prevent error if called on a null object
		if (canvasItem == null)
		{
			GD.PrintErr("SetVisibilityZOrdering extension called on a null CanvasItem.");
			return;
		}

		canvasItem.ZIndex = (int)zOrdering;
	}
}