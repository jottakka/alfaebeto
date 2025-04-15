using Godot;

public static class CanvasItemExtensions
{
	public static void SetVisibilityZOrdering(this CanvasItem canvasItem, VisibilityZOrdering zOrdering) => canvasItem.ZIndex = (int)zOrdering;
}
