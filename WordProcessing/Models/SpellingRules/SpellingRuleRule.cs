using System.Text.Json.Serialization;

namespace WordProcessing.Models.SpellingRules;
public record SpellingRuleRule(
    [property: JsonPropertyName("RuleType")] string RuleTypeString,
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Description")] string Description,
    [property: JsonPropertyName("Words")] IReadOnlyList<SpellingRuleWord> Words
)
{
    [JsonIgnore]
    public SpellingRuleRuleType RuleType => RuleTypeString switch
    {
        "SemRegraS" => SpellingRuleRuleType.SemRegraS,
        "SufixosOrigem" => SpellingRuleRuleType.SufixosOrigem,
        "PrefixoDes" => SpellingRuleRuleType.PrefixoDes,
        "SufixosOso" => SpellingRuleRuleType.SufixosOso,
        "VerbosPorQuerer" => SpellingRuleRuleType.VerbosPorQuerer,
        "VerbosComS" => SpellingRuleRuleType.VerbosComS,
        "SemRegraZ" => SpellingRuleRuleType.SemRegraZ,
        "ExcecoesRegraS" => SpellingRuleRuleType.ExcecoesRegraS,
        "SufixosIzar" => SpellingRuleRuleType.SufixosIzar,
        "SufixosEzEza" => SpellingRuleRuleType.SufixosEzEza,
        "SemRegraX" => SpellingRuleRuleType.SemRegraX,
        "DitongoX" => SpellingRuleRuleType.DitongoX,
        "OrigemIndigenaAfricanaInglesa" => SpellingRuleRuleType.OrigemIndigenaAfricanaInglesa,
        "EnX" => SpellingRuleRuleType.EnX,
        "MeX" => SpellingRuleRuleType.MeX,
        "SemRegraCH" => SpellingRuleRuleType.SemRegraCH,
        "EstrangeirismoCH" => SpellingRuleRuleType.EstrangeirismoCH,
        "MechaCH" => SpellingRuleRuleType.MechaCH,
        "VerboEncherCH" => SpellingRuleRuleType.VerboEncherCH,
        _ => throw new ArgumentException("Invalid RuleType")
    };
}