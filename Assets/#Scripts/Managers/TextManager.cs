using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityHelper;
using System.Collections.Generic;

public static class TextManager
{
    public static List<ColorfulChar> GenerateText(string databasePath, Color color, int wordsCount)
    {
        List<string> words = JsonUtility.FromJson<WordsDatabase>(File.ReadAllText(databasePath)).words;

        List<string> text = new();

        for (int i = 0; i < wordsCount; i++)
            text.Add(words[UnityEngine.Random.Range(0, words.Count)].ToLower());

        return String.Join(' ', text).Select(w => new ColorfulChar(w, color)).ToList();
    }

    [Serializable]
    private class WordsDatabase
    {
        public List<string> words;
    }
}