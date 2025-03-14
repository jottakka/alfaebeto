using System.Text.Json.Serialization;

namespace WordProcessing.Models.DiacriticalMarks;

public record DiactricalMarkCategories(
    [property: JsonPropertyName("Categorias")] IReadOnlyList<DiactralMarkCategory> Categories,
    [property: JsonConverter(typeof(DiactricalMarkWordListConverter))]
    [property: JsonPropertyName("PalavrasSem")] IReadOnlyList<DiactricalMarkWordInfo> NotMarkedWords
);
