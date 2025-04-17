using Godot;

namespace AlfaEBetto.Extensions
{
	public static class CanvasItemExtensions
	{
		public static void SetVisibilityZOrdering(this CanvasItem canvasItem, VisibilityZOrdering zOrdering) => canvasItem.ZIndex = (int)zOrdering;
	}
}
