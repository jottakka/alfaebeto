using System.Text.Json.Serialization;

namespace WordProcessing.Models.XorCH;

public record Rule(
[property: JsonConverter(typeof(RuleTypeEnumConverter))]
[property: JsonPropertyName("RuleType")] RuleType RuleType,
[property: JsonPropertyName("Description")] string Description,
[property: JsonPropertyName("Words")] IReadOnlyList<XorCHWord> Words);
