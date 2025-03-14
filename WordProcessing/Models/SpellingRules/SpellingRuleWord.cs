using System.Text.Json.Serialization;

namespace WordProcessing.Models.SpellingRules;
public record SpellingRuleWord(
    [property: JsonPropertyName("Word")] string Original,
    [property: JsonPropertyName("RightOption")] string RightOption,
    [property: JsonPropertyName("FirstPart")] string FirstPart,
    [property: JsonPropertyName("SecondPart")] string SecondPart,
    [property: JsonPropertyName("Options")] IReadOnlyList<string> Options,
    [property: JsonPropertyName("Indices")] IReadOnlyList<int> Indices
);