using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace AlfaEBetto.Blocks;
public static class GermanGenderExtensions
{
	/// <summary>
	/// Returns a distinct color for each German article,
	/// chosen to look good on a light grey background.
	/// </summary>
	public static Color ToColor(this WordGender gender)
	{
		switch (gender)
		{
			case WordGender.Masculine:
				// A medium-blue shade
				return new Color("#2E86C1");
			case WordGender.Feminine:
				// A moderately bright red shade
				return new Color("#C0392B");
			case WordGender.Neuter:
				// A green shade
				return new Color("#27AE60");
			default:
				// Fallback color
				return new Color("#000000"); // black
		}
	}
}