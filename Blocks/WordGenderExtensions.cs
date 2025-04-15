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
		return gender switch
		{
			WordGender.Masculine => new Color("#2E86C1"),// A medium-blue shade
			WordGender.Feminine => new Color("#C0392B"),// A moderately bright red shade
			WordGender.Neuter => new Color("#27AE60"),// A green shade
			_ => new Color("#000000"),// Fallback color
									  // black
		};
	}
}