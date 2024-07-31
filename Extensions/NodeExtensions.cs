using Godot;


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
