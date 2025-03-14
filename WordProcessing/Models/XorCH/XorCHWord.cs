using System.Text.Json.Serialization;

namespace WordProcessing.Models.XorCH;

public record XorCHWord(
[property: JsonPropertyName("Word")] string Text,
[property: JsonPropertyName("RightOption")] string RightOption,
[property: JsonPropertyName("Indices")] IReadOnlyList<int> Indices,
[property: JsonPropertyName("FirstPart")] string FirstPart,
[property: JsonPropertyName("SecondPart")] string SecondPart
);
