namespace WordProcessing.Util;

public record GermanWordResource
{
	public string Word { get; set; }
	public string Article { get; set; }

	public GermanWordResource(string word, string article)
	{
		Word = word;
		Article = article;
	}
}

public sealed record GuessArticleWordResource
{
	public string ToBeGuessed { get; init; } = "";
	public int AnswerIdx { get; init; }
	public string[] ArticleOptions { get; init; } = [];
}

public static class GermanGenderResourceProvider
{
	private static readonly List<GermanWordResource> Words = new()
	{
        // Masculine (Der)
        new("Apfel", "Der"), new("Baum", "Der"), new("Ball", "Der"), new("Berg", "Der"),
		new("Hund", "Der"), new("Vogel", "Der"), new("Stuhl", "Der"), new("Tisch", "Der"),
		new("Kaffee", "Der"), new("Tag", "Der"), new("Zug", "Der"), new("Fluss", "Der"),
		new("Löffel", "Der"), new("Bleistift", "Der"), new("Computer", "Der"), new("Bahnhof", "Der"),
		new("Film", "Der"), new("Satz", "Der"), new("Würfel", "Der"), new("Monat", "Der"),
		new("Garten", "Der"), new("Fußball", "Der"), new("Kuchen", "Der"), new("Pullover", "Der"),
		new("Schuh", "Der"), new("Rock", "Der"), new("Mantel", "Der"), new("Brief", "Der"),
		new("Arm", "Der"), new("Finger", "Der"), new("Kopf", "Der"), new("Fuß", "Der"),
		new("Himmel", "Der"), new("Schlüssel", "Der"), new("Topf", "Der"), new("Salat", "Der"),
		new("Hut", "Der"), new("Wagen", "Der"), new("Wind", "Der"), new("See", "Der"),

        // Feminine (Die)
        new("Banane", "Die"), new("Katze", "Die"), new("Straße", "Die"), new("Blume", "Die"),
		new("Tasche", "Die"), new("Gabel", "Die"), new("Sonne", "Die"), new("Lampe", "Die"),
		new("Zeitung", "Die"), new("Uhr", "Die"), new("Wolke", "Die"), new("Schule", "Die"),
		new("Karte", "Die"), new("Minute", "Die"), new("Sekunde", "Die"), new("Frucht", "Die"),
		new("Maus", "Die"), new("Wiese", "Die"), new("Pflanze", "Die"), new("Uhrzeit", "Die"),
		new("Jacke", "Die"), new("Schokolade", "Die"), new("Butter", "Die"), new("Milch", "Die"),
		new("Pizza", "Die"), new("Tasse", "Die"), new("Tafel", "Die"), new("Kreide", "Die"),
		new("Tür", "Die"), new("Brücke", "Die"), new("Hand", "Die"), new("Nase", "Die"),
		new("Brille", "Die"), new("Küche", "Die"), new("Mütze", "Die"), new("Flasche", "Die"),
		new("Hose", "Die"), new("Farbe", "Die"), new("Kerze", "Die"), new("Party", "Die"),

        // Neuter (Das)
        new("Buch", "Das"), new("Auto", "Das"), new("Fenster", "Das"), new("Kind", "Das"),
		new("Haus", "Das"), new("Bett", "Das"), new("Mädchen", "Das"), new("Telefon", "Das"),
		new("Eis", "Das"), new("Brot", "Das"), new("Wasser", "Das"), new("Meer", "Das"),
		new("Tal", "Das"), new("Obst", "Das"), new("Flugzeug", "Das"), new("Restaurant", "Das"),
		new("Kino", "Das"), new("Spiel", "Das"), new("Gemüse", "Das"), new("Gebäude", "Das"),
		new("Licht", "Das"), new("Glas", "Das"), new("Heft", "Das"), new("Papier", "Das"),
		new("Bild", "Das"), new("Foto", "Das"), new("Zimmer", "Das"), new("Lied", "Das"),
		new("Gesicht", "Das"), new("Tier", "Das"), new("Baby", "Das"), new("Museum", "Das"),
		new("Land", "Das"), new("Zelt", "Das"), new("Geld", "Das"), new("Problem", "Das"),
		new("Feuer", "Das"), new("Kissen", "Das"), new("Geschenk", "Das"), new("Fahrrad", "Das")
	};

	private static readonly string[] AllOptions = ["Der", "Die", "Das"];
	private static readonly Random _rng = new();

	public static GuessArticleWordResource GetRandomResource()
	{
		var word = Words[_rng.Next(Words.Count)];
		int correctIdx = Array.IndexOf(AllOptions, word.Article);

		return new GuessArticleWordResource
		{
			ToBeGuessed = word.Word,
			AnswerIdx = correctIdx,
			ArticleOptions = AllOptions
		};
	}
}

