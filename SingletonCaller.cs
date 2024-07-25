using Godot;
public sealed partial class SingletonCaller : Node
{
    Global Global { get; set; }


    public override void _Ready()
    {
        Global = GetNode<Global>($"/root/{nameof(Global)}");
    }
}
