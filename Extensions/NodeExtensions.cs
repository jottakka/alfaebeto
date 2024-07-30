using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class NodeExtensions
{
    public static void AddChildDeffered(
        this Node parent,
        Node child
       )
    {
        parent.CallDeferred(
            Node.MethodName.AddChild,
            child
        );
    }
}
