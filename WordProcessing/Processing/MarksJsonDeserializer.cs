using System.Text.Json;
using WordProcessing.Models.DiacriticalMarks;

namespace WordProcessing.Processing;

public static class MarksJsonDeserializer
{
	public static DiactricalMarkCategories DeserializeJsonString(string jsonString)
	{
		JsonSerializerOptions options = new()
		{
			Converters = { new DiactricalMarkWordListConverter() },
			WriteIndented = true
		};

		DiactricalMarkCategories categorias = JsonSerializer.Deserialize<DiactricalMarkCategories>(jsonString, options);

		return categorias;
	}
}
