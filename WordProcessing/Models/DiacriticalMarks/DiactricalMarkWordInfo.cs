namespace WordProcessing.Models.DiacriticalMarks;

public record DiactricalMarkWordInfo(
	string Original,
	string WithoutDiacritics,
	bool HasMark,
	int? DiacriticIndex = null
);
