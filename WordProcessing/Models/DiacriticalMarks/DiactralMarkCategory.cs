using System.Text.Json.Serialization;

namespace WordProcessing.Models.DiacriticalMarks;

public record DiactralMarkCategory(
    [property: JsonPropertyName("Nome")] string Name,
    [property: JsonPropertyName("Descrição")] string Description,
    [property: JsonPropertyName("Subcategorias")] IReadOnlyList<DiactricalMarkSubCategory> Subcategories
)
{
    public DiactricalMarkRuleSetType Type => EnumMapping[Name];

    private static readonly IReadOnlyDictionary<string, DiactricalMarkRuleSetType> EnumMapping = new Dictionary<string, DiactricalMarkRuleSetType>
    {
        { "Monossílabas Tônicas", DiactricalMarkRuleSetType.MonossilabasTonicas },
        { "Oxítonas", DiactricalMarkRuleSetType.Oxiotonas },
        { "Paroxítonas", DiactricalMarkRuleSetType.Paroxitonas },
        { "Proparoxítonas", DiactricalMarkRuleSetType.Proparoxitonas },
        { "Ditongos Abertos", DiactricalMarkRuleSetType.DitongosAbertos },
        { "Hiatos I e U Tônicos", DiactricalMarkRuleSetType.HiatosIeUTonicos }
    };
}

