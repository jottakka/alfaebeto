using Godot;

public sealed partial class SceneManager : Node
{

    private Node Root => GetTree().Root;

    public void SwitchScene(string scenePath)
    {
    }
}
