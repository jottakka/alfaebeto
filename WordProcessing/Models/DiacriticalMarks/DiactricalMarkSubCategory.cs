using System.Text.Json.Serialization;

namespace WordProcessing.Models.DiacriticalMarks;

public record DiactricalMarkSubCategory(
	[property: JsonPropertyName("Nome")] string Name,
	[property: JsonPropertyName("Descrição")] string Description,
	[property: JsonConverter(typeof(DiactricalMarkWordListConverter))]
	[property: JsonPropertyName("Palavras")] IReadOnlyList<DiactricalMarkWordInfo> Words
)
{
	public DiactricalMarkRuleType Type => EnumMapping[Name];

	private static readonly IReadOnlyDictionary<string, DiactricalMarkRuleType> EnumMapping = new Dictionary<string, DiactricalMarkRuleType>
	{
		{ "Terminadas em -a(s)", DiactricalMarkRuleType.TerminadasEmAs },
		{ "Terminadas em -e(s)", DiactricalMarkRuleType.TerminadasEmEs },
		{ "Terminadas em -o(s)", DiactricalMarkRuleType.TerminadasEmOs },
		{ "Terminadas em -em", DiactricalMarkRuleType.TerminadasEmEm },
		{ "Terminadas em -ens", DiactricalMarkRuleType.TerminadasEmEns },
		{ "Terminadas em -l", DiactricalMarkRuleType.TerminadasEmL },
		{ "Terminadas em -i(s)", DiactricalMarkRuleType.TerminadasEmIs },
		{ "Terminadas em -n", DiactricalMarkRuleType.TerminadasEmN },
		{ "Terminadas em -u(s)", DiactricalMarkRuleType.TerminadasEmUs },
		{ "Terminadas em -r", DiactricalMarkRuleType.TerminadasEmR },
		{ "Terminadas em -x", DiactricalMarkRuleType.TerminadasEmX },
		{ "Terminadas em -ã(s)", DiactricalMarkRuleType.TerminadasEmA },
		{ "Terminadas em -ão(s)", DiactricalMarkRuleType.TerminadasEmAo },
		{ "Terminadas em -um", DiactricalMarkRuleType.TerminadasEmUm },
		{ "Terminadas em -uns", DiactricalMarkRuleType.TerminadasEmUns },
		{ "Terminadas em -ps", DiactricalMarkRuleType.TerminadasEmPs },
		{ "Terminadas em Ditongo", DiactricalMarkRuleType.TerminadasEmDitongo },
		{ "Todas Proparoxítonas", DiactricalMarkRuleType.TodasProparoxitonas },
		{ "Terminadas em -éi", DiactricalMarkRuleType.TerminadasEmEi },
		{ "Terminadas em -éu", DiactricalMarkRuleType.TerminadasEmEu },
		{ "Terminadas em -ói", DiactricalMarkRuleType.TerminadasEmOi },
		{ "Sozinhos na Sílaba", DiactricalMarkRuleType.SozinhosNaSilaba },
		{ "Precedidos de Vogais", DiactricalMarkRuleType.PrecedidosDeVogais }
	};
}
