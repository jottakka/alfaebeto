using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CanvasItemExtensions
{
    public static void SetVisibilityZOrdering(this CanvasItem canvasItem, VisibilityZOrdering zOrdering)
    {
        canvasItem.ZIndex = (int)zOrdering;
    }
}
