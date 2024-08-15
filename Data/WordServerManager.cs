using System;
using System.Collections.Generic;
using System.Linq;

public sealed class WordServerManager
{
    private DiactricalMarkWordsDataResource _diactricalMarkWordResource => Global.Instance.DiactricalMarkWordsDataResource;
    private UserDataInfoResource _userDataInfo => Global.Instance.UserDataInfoResource;
    private SpellingRulesResource _spellingRulesResource => Global.Instance.SpellingRulesResource;

    public Queue<SpellingRuleWordResource> GetShuffledSpellingRuleWords(int take = 10)
    {
        IEnumerable<SpellingRuleWordResource> unlockedSpellingRulesWords =
            GetUnlockedSpellingRuleWordsForUser();

        IEnumerable<SpellingRuleWordResource> shuffledWords =
            GetShuffledWords(unlockedSpellingRulesWords, take);

        return new Queue<SpellingRuleWordResource>(shuffledWords);
    }

    public Queue<DiactricalMarkWordResource> GetShuffledDiactricalMarkWords(int take = 10)
    {
        IEnumerable<DiactricalMarkWordResource> shuffledMarkedWords = GetShuffledMarkedWords(take / 2);
        IEnumerable<DiactricalMarkWordResource> shuffledNotMarkedWords = GetShuffledNotMarkedWords((take + 1) / 2);

        IOrderedEnumerable<DiactricalMarkWordResource> concatedLists = shuffledNotMarkedWords
            .Concat(shuffledMarkedWords)
            .OrderBy(w => Random.Shared.Next());
        return new Queue<DiactricalMarkWordResource>(concatedLists);
    }

    private IEnumerable<DiactricalMarkWordResource> GetShuffledNotMarkedWords(int take)
    {
        return GetShuffledWords(
            _diactricalMarkWordResource.NotMarkedWords,
            take
        );
    }

    private IEnumerable<DiactricalMarkWordResource> GetShuffledMarkedWords(int take)
    {
        IEnumerable<DiactricalMarkWordResource> markedWords = GetUnlockedMarkedWordsForUser();
        IEnumerable<DiactricalMarkWordResource> shuffledMarkedWords = GetShuffledWords(
            markedWords,
            take
        );
        return shuffledMarkedWords;
    }

    private IEnumerable<DiactricalMarkWordResource> GetUnlockedMarkedWordsForUser()
    {
        return _userDataInfo.UnlockedDiactricalMarksSubCategories.SelectMany(
            subCatType => _diactricalMarkWordResource.MarkedWordsByRule[subCatType]);
    }

    private IEnumerable<SpellingRuleWordResource> GetUnlockedSpellingRuleWordsForUser()
    {
        //return _userDataInfo.UnlockedSpellingRuleRuleTypes.SelectMany(
        //    ruleType => _spellingRulesResource.WordsByRule[ruleType]);
        return _spellingRulesResource.WordsByRule.Values.SelectMany(v => v);
    }

    private IEnumerable<TWord> GetShuffledWords<TWord>(IEnumerable<TWord> words, int take)
    {
        return words
            .OrderBy(w => Random.Shared.Next())
            .Take(take);
    }
}
