using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using WordProcessing.Processing;
using WordProcessing.Filtering;
using WordProcessing.Models.DiacriticalMarks;

public partial class Global : Node
{
	public static Global Instance { get; private set; } = null;

	public Player Player { get; set; }

	public Node Scene { get; set; }

	public Queue<WordInfo> Words { get; set; }

	public Global()
	{
		string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
		string jsonString = File.ReadAllText(filePath);
		Words = MarksJsonDeserializer
			.DeserializeJsonString(jsonString)
			.GetWordsShuffledByCategory(CategoryEnum.Paroxitonas);
	}

	public override void _Ready()
	{
		if (Instance is not null)
		{
			QueueFree();
			return;
		}
		Instance = this;
		// Initialize random number generator
		GD.Randomize();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
