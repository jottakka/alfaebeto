using Godot;
public sealed partial class SingletonCaller : Node
{
    private Global Global { get; set; }

    public override void _Ready()
    {
        Global = GetNode<Global>($"/root/{nameof(Global)}");
    }
}
