namespace WordProcessing.Util;

public sealed record PickRightOptionFromHintData(
	string ToBeGuessed,
	int AnswerIdx,
	string[] ShuffledOptions
	);
