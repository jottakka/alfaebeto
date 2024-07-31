using Godot;

public static class NodeExtensions
{
    public static void AddChildDeffered(
        this Node parent,
        Node child
       )
    {
        _ = parent.CallDeferred(
            Node.MethodName.AddChild,
            child
        );
    }
}
