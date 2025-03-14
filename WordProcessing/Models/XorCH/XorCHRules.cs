using System.Text.Json.Serialization;

namespace WordProcessing.Models.XorCH;
public record XorCHRules(
[property: JsonPropertyName("Options")] IReadOnlyList<string> Options,
[property: JsonPropertyName("Rules")] IReadOnlyList<Rule> Rules
);
