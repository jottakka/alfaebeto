using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordProcessing.Filtering;
using WordProcessing.Models.XorCH;
using WordProcessing.Processing;

public sealed class WordServerManager
{
    private readonly IEnumerable<XorCHWord> _unlockedXorChWords;
    private DiactricalMarkWordsDataResource _diactricalMarkWordResource => Global.Instance.DiactricalMarkWordsDataResource;
    private UserDataInfoResource _userDataInfo => Global.Instance.UserDataInfoResource;

    public WordServerManager()
    {
        string filePath = @"C:\git\alfa_e_betto\Data\acentuação\ch_e_x_proper.json";
        string jsonString = File.ReadAllText(filePath);
        _unlockedXorChWords = XorCHDeserializer
            .DeserializeJsonString(jsonString)
            .GetWordInfosByCategories();
    }

    public Queue<XorCHWord> GetShuffledXorCHWords(int take = 10)
    {
        IEnumerable<XorCHWord> shuffledWords = GetShuffledWords(_unlockedXorChWords, take);
        return new Queue<XorCHWord>(shuffledWords);
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
    private IEnumerable<TWord> GetShuffledWords<TWord>(IEnumerable<TWord> words, int take)
    {
        return words
            .OrderBy(w => Random.Shared.Next())
            .Take(take);
    }
}
