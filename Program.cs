﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NaiveBayesClassifier
{
    class Program
    {
        static List<Document> training = new List<Document>
        {
            new Document("ham", "hi. how are you"),
            new Document("ham", "how old are you"),
            new Document("spam", "bye milk")
        };
        static string newmsg = "bye, water";

        static void Main(string[] args)
        {
            var c = new Classifier(training);
            var res = c.IsInClassProbability("spam", newmsg);
            var res2 = c.IsInClassProbability("ham", newmsg);
            Console.WriteLine("probability of spam: " + res);
            Console.ReadKey();
        }
    }
    class Document
    {
        public Document(string @class, string text)
        {
            Class = @class;
            Text = text;
        }
        public string Class { get; set; }
        public string Text { get; set; }
    }
    public static class Helpers //убирает знаки препинания
    {
        public static List<String> ExtractFeatures(this String text)
        {
            return Regex.Replace(text, "\\p{P}+", "").Split(' ').ToList();
        }
    }
    
    //количество слов в спаме и не спаме
    class ClassInfo 
    {
        public ClassInfo(string name, List<String> trainDocs)
        {
            Name = name;
            var features = trainDocs.SelectMany(x => x.ExtractFeatures());
            WordsCount = features.Count();
            WordCount =
                features.GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            NumberOfDocs = trainDocs.Count;
        }
        public string Name { get; set; }
        public int WordsCount { get; set; }
        public Dictionary<string, int> WordCount { get; set; }
        public int NumberOfDocs { get; set; }
        public int NumberOfOccurencesInTrainDocs(String word)
        {
            if (WordCount.Keys.Contains(word)) return WordCount[word];
            return 0;
        }
        
    }

    class Classifier
    {
        List<ClassInfo> _classes;
        int _countOfDocs;
        int _uniqWordsCount; 
        public Classifier(List<Document> train)//подсчет слов в спаме и не спаме
        {
            _classes = train.GroupBy(x => x.Class).Select(g => new ClassInfo(g.Key, g.Select(x => x.Text).ToList())).ToList();
            _countOfDocs = train.Count;
            _uniqWordsCount = train.SelectMany(x => x.Text.Split(' ')).GroupBy(x => x).Count();
        }

        public double IsInClassProbability(string className, string text)//вероятность быть в classname
        {
            var words = text.ExtractFeatures();
            var classResults = _classes
                .Select(x => new
                {
                    Result = Math.Pow(Math.E, Calc(x.NumberOfDocs, _countOfDocs, words, x.WordsCount, x, _uniqWordsCount)),
                    ClassName = x.Name
                });
            
            return classResults.Single(x => x.ClassName == className).Result / classResults.Sum(x => x.Result);
        }

        private static double Calc(double dc, double d, List<String> q, double lc, ClassInfo @class, double v)
        {
            return Math.Log(dc / d) + q.Sum(x => Math.Log((@class.NumberOfOccurencesInTrainDocs(x) + 1) / (v + lc)));
        }
    }
}