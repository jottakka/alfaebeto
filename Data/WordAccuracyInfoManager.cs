using System.Collections.Generic;
using System.Linq;

namespace AlfaEBetto.Data;
public sealed class WordAccuracyInfoManager
{
    private readonly Dictionary<string, WordAccuracyInfo> _wordAccuracyInfos = new();

    public void AddWordAccuracyInfo(WordAccuracyInfo wordAccuracyInfo)
    {
        if (_wordAccuracyInfos.ContainsKey(wordAccuracyInfo.Word))
        {
            _wordAccuracyInfos[wordAccuracyInfo.Word] = wordAccuracyInfo;
        }
        else
        {
            _wordAccuracyInfos.Add(wordAccuracyInfo.Word, wordAccuracyInfo);
        }
    }

    public WordAccuracyInfo GetWordAccuracyInfo(string word)
    {
        return _wordAccuracyInfos.ContainsKey(word) ? _wordAccuracyInfos[word] : null;
    }

    public void RemoveWordAccuracyInfo(string word)
    {
        if (_wordAccuracyInfos.ContainsKey(word))
        {
            _ = _wordAccuracyInfos.Remove(word);
        }
    }

    public void Clear()
    {
        _wordAccuracyInfos.Clear();
    }

    public List<WordAccuracyInfo> GetWordAccuracyInfos()
    {
        return _wordAccuracyInfos.Values.ToList();
    }
}
