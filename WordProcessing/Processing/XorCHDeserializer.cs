using System.Text.Json;
using WordProcessing.Models.SpellingRules;
using WordProcessing.Models.XorCH;

namespace WordProcessing.Processing;

public static class XorCHDeserializer
{
    public static XorCHRules DeserializeJsonString(string jsonString)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new RuleTypeEnumConverter() }
        };

        XorCHRules rulesData = JsonSerializer.Deserialize<XorCHRules>(jsonString, options);

        return rulesData;
    }

    public static SpellingRuleRoot DeserializeJsonStringSpellingRule(string jsonString)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new RuleTypeEnumConverter() }
        };

        SpellingRuleRoot rulesData = JsonSerializer.Deserialize<SpellingRuleRoot>(jsonString, options);

        return rulesData;
    }
}
