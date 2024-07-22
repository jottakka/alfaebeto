using Godot;
using System;

public partial class Global : Node
{
	public static Global Instance { get; private set; } = null;

	public Player Player { get; set; }

	public Node Scene { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Instance is not null)
		{
			QueueFree();
			return;
		}
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
