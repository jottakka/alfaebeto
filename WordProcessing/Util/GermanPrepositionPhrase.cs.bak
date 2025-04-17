/// <summary>
/// Represents the grammatical gender of a German noun.
/// </summary>
public enum NounGender
{
	Masculine, // der
	Feminine,  // die
	Neuter     // das
}

/// <summary>
/// Represents the grammatical case required by a preposition (or verb/context).
/// </summary>
public enum GrammaticalCase
{
	Accusative, // Wenfall
	Dative,     // Wemfall
	Genitive    // Wesfall
}

/// <summary>
/// Holds data for a German phrase focusing on a preposition, noun, and article selection.
/// Provides exactly two article options: one correct, one incorrect.
/// Assumes singular nouns.
/// </summary>
public record GermanPrepositionPhrase
{
	/// <summary>
	/// The preposition governing the case. E.g., "mit", "für".
	/// </summary>
	public string Preposition { get; init; }

	/// <summary>
	/// The noun used in the phrase. E.g., "Auto", "Mann".
	/// </summary>
	public string Noun { get; init; }

	/// <summary>
	/// The inherent grammatical gender of the Noun.
	/// </summary>
	public NounGender Gender { get; init; }

	/// <summary>
	/// The grammatical case required by this Preposition.
	/// </summary>
	public GrammaticalCase GovernedCase { get; init; }

	/// <summary>
	/// Holds exactly two article options: one correct, one incorrect.
	/// The order is randomized.
	/// </summary>
	public string[] Options { get; init; } = new string[2];

	/// <summary>
	/// The index (0 or 1) within the Options array where the correct article is located.
	/// </summary>
	public int CorrectOptionIndex { get; set; }

	public override string ToString() => $"{Preposition} ... {Noun} (Case: {GovernedCase}, Gender: {Gender}) Options: [{Options[0]}, {Options[1]}] Correct Idx: {CorrectOptionIndex}";
}

public static class GermanPhraseGenerator
{
	// All possible singular definite article forms
	private static readonly List<string> _allArticles = ["der", "die", "das", "den", "dem", "des"];
	private static readonly Random _random = new();

	// Helper to get one random incorrect article, different from the correct one
	private static string GetRandomDistractor(string correctArticle)
	{
		// Get all articles except the correct one
		List<string> possibleDistractors = _allArticles.Except([correctArticle]).ToList();
		if (!possibleDistractors.Any())
		{
			// Should not happen with standard articles, but as a fallback
			return correctArticle == "der" ? "die" : "der";
		}
		// Pick one randomly
		int randomIndex = _random.Next(possibleDistractors.Count);
		return possibleDistractors[randomIndex];
	}
	private static string GetCorrectArticle(NounGender gender, GrammaticalCase grammaticalCase)
	{
		return grammaticalCase switch
		{
			GrammaticalCase.Accusative => gender switch
			{
				NounGender.Masculine => "den",
				NounGender.Feminine => "die",
				NounGender.Neuter => "das",
				_ => "?"
			},
			GrammaticalCase.Dative => gender switch
			{
				NounGender.Masculine => "dem",
				NounGender.Feminine => "der",
				NounGender.Neuter => "dem",
				_ => "?"
			},
			GrammaticalCase.Genitive => gender switch
			{
				NounGender.Masculine => "des",
				NounGender.Feminine => "der",
				NounGender.Neuter => "des",
				_ => "?"
			},
			_ => "?"
		};
	}

	private static readonly List<(string Prep, string Noun, NounGender Gen, GrammaticalCase Case)> _phraseData =
	[
             // --- Dative Prepositions ---
            ("mit", "Mann", NounGender.Masculine, GrammaticalCase.Dative),
			("mit", "Auto", NounGender.Neuter, GrammaticalCase.Dative),
			("mit", "Frau", NounGender.Feminine, GrammaticalCase.Dative),
			("mit", "Kind", NounGender.Neuter, GrammaticalCase.Dative),
			("mit", "Hund", NounGender.Masculine, GrammaticalCase.Dative),
			("mit", "Katze", NounGender.Feminine, GrammaticalCase.Dative),
			("mit", "Buch", NounGender.Neuter, GrammaticalCase.Dative),
			("mit", "Stuhl", NounGender.Masculine, GrammaticalCase.Dative),
			("mit", "Hand", NounGender.Feminine, GrammaticalCase.Dative),
			("nach", "Haus", NounGender.Neuter, GrammaticalCase.Dative),
			("nach", "Schule", NounGender.Feminine, GrammaticalCase.Dative),
			("nach", "Konzert", NounGender.Neuter, GrammaticalCase.Dative),
			("nach", "Arbeit", NounGender.Feminine, GrammaticalCase.Dative),
			("aus", "Büro", NounGender.Neuter, GrammaticalCase.Dative),
			("aus", "Stadt", NounGender.Feminine, GrammaticalCase.Dative),
			("aus", "Flasche", NounGender.Feminine, GrammaticalCase.Dative),
			("aus", "Glas", NounGender.Neuter, GrammaticalCase.Dative),
			("von", "Lehrer", NounGender.Masculine, GrammaticalCase.Dative),
			("von", "Kind", NounGender.Neuter, GrammaticalCase.Dative),
			("von", "Kollegin", NounGender.Feminine, GrammaticalCase.Dative),
			("von", "Direktor", NounGender.Masculine, GrammaticalCase.Dative),
			("zu", "Bahnhof", NounGender.Masculine, GrammaticalCase.Dative),
			("zu", "Tür", NounGender.Feminine, GrammaticalCase.Dative),
			("zu", "Fenster", NounGender.Neuter, GrammaticalCase.Dative),
			("zu", "Supermarkt", NounGender.Masculine, GrammaticalCase.Dative),
			("bei", "Arzt", NounGender.Masculine, GrammaticalCase.Dative),
			("bei", "Freundin", NounGender.Feminine, GrammaticalCase.Dative),
			("bei", "Konzert", NounGender.Neuter, GrammaticalCase.Dative),
			("seit", "Jahr", NounGender.Neuter, GrammaticalCase.Dative),
			("seit", "Monat", NounGender.Masculine, GrammaticalCase.Dative),
			("seit", "Woche", NounGender.Feminine, GrammaticalCase.Dative),
			("außer", "Chef", NounGender.Masculine, GrammaticalCase.Dative),
			("gegenüber", "Kirche", NounGender.Feminine, GrammaticalCase.Dative),

            // --- Accusative Prepositions ---
            ("für", "Mann", NounGender.Masculine, GrammaticalCase.Accusative),
			("für", "Kind", NounGender.Neuter, GrammaticalCase.Accusative),
			("für", "Katze", NounGender.Feminine, GrammaticalCase.Accusative),
			("für", "Hund", NounGender.Masculine, GrammaticalCase.Accusative),
			("für", "Geschenk", NounGender.Neuter, GrammaticalCase.Accusative),
			("für", "Tasche", NounGender.Feminine, GrammaticalCase.Accusative),
			("durch", "Park", NounGender.Masculine, GrammaticalCase.Accusative),
			("durch", "Fenster", NounGender.Neuter, GrammaticalCase.Accusative),
			("durch", "Wald", NounGender.Masculine, GrammaticalCase.Accusative),
			("durch", "Tür", NounGender.Feminine, GrammaticalCase.Accusative),
			("gegen", "Baum", NounGender.Masculine, GrammaticalCase.Accusative),
			("gegen", "Wand", NounGender.Feminine, GrammaticalCase.Accusative),
			("gegen", "Feind", NounGender.Masculine, GrammaticalCase.Accusative),
			("gegen", "Regel", NounGender.Feminine, GrammaticalCase.Accusative),
			("ohne", "Hund", NounGender.Masculine, GrammaticalCase.Accusative),
			("ohne", "Geld", NounGender.Neuter, GrammaticalCase.Accusative),
			("ohne", "Brille", NounGender.Feminine, GrammaticalCase.Accusative),
			("ohne", "Schlüssel", NounGender.Masculine, GrammaticalCase.Accusative),
			("um", "Tisch", NounGender.Masculine, GrammaticalCase.Accusative),
			("um", "Ecke", NounGender.Feminine, GrammaticalCase.Accusative),
			("um", "Hals", NounGender.Masculine, GrammaticalCase.Accusative),
			("um", "Handgelenk", NounGender.Neuter, GrammaticalCase.Accusative),

             // --- Genitive Prepositions ---
            ("wegen", "Regen", NounGender.Masculine, GrammaticalCase.Genitive),
			("trotz", "Problem", NounGender.Neuter, GrammaticalCase.Genitive),
			("während", "Pause", NounGender.Feminine, GrammaticalCase.Genitive),
			("anstatt", "Kaffee", NounGender.Masculine, GrammaticalCase.Genitive),
			("innerhalb", "Gebäude", NounGender.Neuter, GrammaticalCase.Genitive),
			("außerhalb", "Stadt", NounGender.Feminine, GrammaticalCase.Genitive)
	];

	/// <summary>
	/// Gets a single, randomly selected German preposition phrase with options.
	/// </summary>
	/// <returns>A GermanPrepositionPhrase object, or null if no data is available.</returns>
	public static GermanPrepositionPhrase GetRandomPhrase()
	{
		if (_phraseData == null || !_phraseData.Any())
		{
			// GD.PrintErr("German phrase data is empty!"); // Use Godot logging if in Godot context
			Console.Error.WriteLine("German phrase data is empty!"); // Use Console if potentially outside Godot
			return null; // Return null if no data to choose from
		}

		// 1. Select random base data
		int randomIndex = _random.Next(_phraseData.Count);
		(string Prep, string Noun, NounGender Gen, GrammaticalCase Case) data = _phraseData[randomIndex];

		// 2. Determine correct article
		string correctArticle = GetCorrectArticle(data.Gen, data.Case);
		if (correctArticle == "?")
		{
			// GD.PrintErr($"Could not determine correct article for {data.Noun}");
			Console.Error.WriteLine($"Could not determine correct article for {data.Noun}");
			return null; // Skip if article couldn't be determined for this entry
		}

		// 3. Get a wrong article as a distractor
		string wrongArticle = GetRandomDistractor(correctArticle);

		// 4. Create the phrase object
		GermanPrepositionPhrase phrase = new()
		{
			Preposition = data.Prep,
			Noun = data.Noun,
			Gender = data.Gen,
			GovernedCase = data.Case,
			Options = new string[2] // Initialize the array
		};

		// 5. Randomly assign correct/wrong to index 0 or 1
		if (_random.Next(2) == 0) // Place correct answer at index 0
		{
			phrase.Options[0] = correctArticle;
			phrase.Options[1] = wrongArticle;
			phrase.CorrectOptionIndex = 0;
		}
		else // Place correct answer at index 1
		{
			phrase.Options[0] = wrongArticle;
			phrase.Options[1] = correctArticle;
			phrase.CorrectOptionIndex = 1;
		}

		return phrase;
	}

	/// <summary>
	/// Generates the complete list of phrase objects with randomized options.
	/// Useful for debugging or specific scenarios, but GetRandomPhrase() is
	/// generally more efficient if you only need one at a time.
	/// </summary>
	/// <returns>A list of all generated GermanPrepositionPhrase objects.</returns>
	public static List<GermanPrepositionPhrase> GetAllGeneratedPhrases()
	{
		List<GermanPrepositionPhrase> generatedPhrases = [];

		if (_phraseData == null)
		{
			return generatedPhrases; // Return empty list if no base data
		}

		foreach ((string Prep, string Noun, NounGender Gen, GrammaticalCase Case) data in _phraseData)
		{
			string correctArticle = GetCorrectArticle(data.Gen, data.Case);
			if (correctArticle == "?")
			{
				continue; // Skip if article couldn't be determined
			}

			string wrongArticle = GetRandomDistractor(correctArticle);

			GermanPrepositionPhrase phrase = new()
			{
				Preposition = data.Prep,
				Noun = data.Noun,
				Gender = data.Gen,
				GovernedCase = data.Case,
				Options = new string[2]
			};

			if (_random.Next(2) == 0)
			{
				phrase.Options[0] = correctArticle;
				phrase.Options[1] = wrongArticle;
				phrase.CorrectOptionIndex = 0;
			}
			else
			{
				phrase.Options[0] = wrongArticle;
				phrase.Options[1] = correctArticle;
				phrase.CorrectOptionIndex = 1;
			}

			generatedPhrases.Add(phrase);
		}

		return generatedPhrases;
	}
}
