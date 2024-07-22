using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
public sealed partial class SingletonCaller : Node
{
	Global Global { get; set; }


	public override void _Ready()
	{
		Global = GetNode<Global>($"/root/{nameof(Global)}");
	}
}
