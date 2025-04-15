using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordProcessing.Models.DiacriticalMarks;

public sealed class DiactricalMarkWordListConverter : JsonConverter<IReadOnlyList<DiactricalMarkWordInfo>>
{
	private static readonly Dictionary<char, char> Replacements = new()
	{
		{'á', 'a'}, {'â', 'a'},
		{'é', 'e'}, {'ê', 'e'},
		{'í', 'i'}, {'î', 'i'},
		{'ú', 'u'}, {'û', 'u'},
		{'ó', 'o'}, {'ô', 'o'},
	};

	public override IReadOnlyList<DiactricalMarkWordInfo> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		List<DiactricalMarkWordInfo> words = [];

		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			string originalWord = reader.GetString()!;
			(string modifiedWord, int? index) = ReplaceAccentedVowelInWord(originalWord);

			words.Add(new DiactricalMarkWordInfo(originalWord, modifiedWord, index.HasValue, index));
		}

		return words;
	}

	private static (string, int?) ReplaceAccentedVowelInWord(string word)
	{
		string lowerWord = word.ToLower();
		System.Text.StringBuilder result = new(lowerWord);

		for (int i = 0; i < lowerWord.Length; i++)
		{
			if (Replacements.ContainsKey(lowerWord[i]))
			{
				result[i] = Replacements[lowerWord[i]];
				return (result.ToString(), i);
			}
		}

		return (word, null);
	}

	public override void Write(Utf8JsonWriter writer, IReadOnlyList<DiactricalMarkWordInfo> value, JsonSerializerOptions options) => throw new NotImplementedException();
}
