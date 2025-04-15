using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordProcessing.Models.XorCH;

public class RuleTypeEnumConverter : JsonConverter<RuleType>
{
	public override RuleType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string value = reader.GetString();
		return value switch
		{
			"VerboEncher" => RuleType.VerboEncher,
			"UsadoAposDitongos" => RuleType.UsadoAposDitongos,
			"OrigemIndigenaAfricanaInglesa" => RuleType.OrigemIndigenaAfricanaInglesa,
			"AposEn" => RuleType.AposEn,
			"RepresentaSomKs" => RuleType.RepresentaSomKs,
			"ChSomSh" => RuleType.ChSomSh,
			"OrigemEstrangeiraAdaptada" => RuleType.OrigemEstrangeiraAdaptada,
			_ => throw new ArgumentException("Invalid RuleType value")
		};
	}

	public override void Write(Utf8JsonWriter writer, RuleType value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
