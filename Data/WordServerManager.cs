using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WordProcessing.Filtering;
using WordProcessing.Models.DiacriticalMarks;
using WordProcessing.Models.XorCH;
using WordProcessing.Processing;

public sealed class WordServerManager
{
    private readonly UserDataInfoResource _userDataInfo;

    private IEnumerable<DiactricalMarkWordInfo> _unlockedMarkedWords;

    private IEnumerable<XorCHWord> _unlockedXorChWords;

    public WordServerManager()
    {
        _userDataInfo ??= new UserDataInfoResource();
        _userDataInfo.UnlockedDiactricalMarksSubCategories = new Godot.Collections.Array<DiactricalMarkSubCategoryType>(
            DiactricalMarkSubCategoryType
            .GetValues(typeof(DiactricalMarkSubCategoryType))
            .Cast<DiactricalMarkSubCategoryType>()
        );

        GetWordData();
    }

    private void GetWordData()
    {
        string filePath = @"C:\git\alfa_e_betto\Data\acentuação\acentos_dados.json";
        string jsonString = File.ReadAllText(filePath);

        DiactricalMarkCategories markedWords = MarksJsonDeserializer.DeserializeJsonString(jsonString);
        _unlockedMarkedWords = markedWords
            .GetWordInfosBySubCategories(_userDataInfo.UnlockedDiactricalMarksSubCategories.ToArray());

        filePath = @"C:\git\alfa_e_betto\Data\acentuação\ch_e_x_proper.json";
        jsonString = File.ReadAllText(filePath);
        _unlockedXorChWords = XorCHDeserializer
            .DeserializeJsonString(jsonString)
            .GetWordInfosByCategories();
    }

    public Queue<XorCHWord> GetShuffledXorCHWords(int take = 10)
    {
        return GetShuffledWords(_unlockedXorChWords, take);
    }

    public Queue<DiactricalMarkWordInfo> GetShuffledDiactricalMarkWords(int take = 10)
    {
        return GetShuffledWords(_unlockedMarkedWords, take);
    }
    private Queue<TWord> GetShuffledWords<TWord>(IEnumerable<TWord> words, int take)
    {
        IEnumerable<TWord> shuffedWords = words
            .OrderBy(w => Random.Shared.Next())
            .Take(take);

        return new Queue<TWord>(shuffedWords);
    }
}
