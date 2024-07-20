using Godot;
using System;

public partial class LetterBlock : StaticBody2D
{
	[Export]
	public Sprite2D Sprite { get; set; }
	[Export]
	public Label Label { get; set; }
	[Export]
	public CollisionShape2D CollisionShape { get; set; }
	[Export]
	AnimationPlayer AnimationPlayer { get; set; }

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetLabel(char letter)
	{
		Label.Text = letter.ToString();
	}

	public void SetPosition(Vector2 position)
	{
		Position = position;
	}
}
