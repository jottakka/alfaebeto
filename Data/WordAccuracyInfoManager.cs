using System.Collections.Generic;

namespace AlfaEBetto.Data;

public sealed class WordAccuracyInfoManager
{
	private readonly UserDataInfoResource _userDataInfo;

	public WordAccuracyInfoManager(UserDataInfoResource userDataInfo) => _userDataInfo = userDataInfo;

	public void UpdateUserWordsResultsData(IEnumerable<WordGameResultItem> gameResults)
	{
		foreach (WordGameResultItem gameResult in gameResults)
		{
			if (_userDataInfo.WordsCategoryInfos.TryGetValue(gameResult.RuleType, out WordCategoryInfoResource wordCategoryInfo))
			{
				UpdateWordCategoryInfo(gameResult, wordCategoryInfo);
			}
			else
			{
				AddWordCategoryInfo(gameResult);
			}
		}
	}

	private void AddWordCategoryInfo(WordGameResultItem gameResult)
	{
		WordCategoryInfoResource newCategoryInfo = new()
		{
			RuleType = gameResult.RuleType,
		};

		AddNewWordAccuracyInfo(gameResult, newCategoryInfo);

		_userDataInfo.WordsCategoryInfos[gameResult.RuleType] = newCategoryInfo;
	}

	private void UpdateWordCategoryInfo(WordGameResultItem gameResult, WordCategoryInfoResource wordCategoryInfo)
	{
		if (wordCategoryInfo.WordAccuracyInfos.TryGetValue(gameResult.Word, out WordAccuracyInfoResource wordAccuracyInfo))
		{
			wordAccuracyInfo.Errors += gameResult.Errors;
			wordAccuracyInfo.Successes += gameResult.Successes;
		}
		else
		{
			AddNewWordAccuracyInfo(gameResult, wordCategoryInfo);
		}
	}

	private void AddNewWordAccuracyInfo(WordGameResultItem gameResult, WordCategoryInfoResource newCategoryInfo)
	{
		newCategoryInfo.WordAccuracyInfos[gameResult.Word] = new()
		{
			Word = gameResult.Word,
			Errors = gameResult.Errors,
			Successes = gameResult.Successes,
			RuleType = gameResult.RuleType,
		};
	}
}
