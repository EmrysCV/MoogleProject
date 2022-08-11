using System;
using System.Diagnostics;
using System.IO;
using System.Text;

/*
Stopwatch cronos = new Stopwatch();
cronos.Start();
(List<string>, string[]) content = LoadDocuments();
TextProcess Data = new TextProcess(content.Item1);
cronos.Stop();
System.Console.WriteLine("ready" + (double)cronos.ElapsedMilliseconds/1000);
Search("Harry Potter", Data);
*/

namespace MoogleEngine;

class Search // Cambiar los modificardores de acceso
{
    public string query;
    public string suggestion;
    public int QUERY_WORDS_AMOUNT;
    public (double, int)[] result;

    public Search(string query, TextProcess Data)
    {/*
        char[] spliters = { ' ', '\n', '\t', ',', '.', ':', ';' };
        string[] normalizedQuery = query.Split(spliters);*/
        this.query = query;
        List<string> normalizedQuery = Tools.Normalize(query, true);

        //int DOCUMENTS_AMOUNT = Data.tfIdf[0].Count;
        int DOCUMENTS_AMOUNT = Data.DOCUMENTS_AMOUNT;
        suggestion = "";
        string closestWord;
        int _wordIndex;

        this.result = new (double, int)[DOCUMENTS_AMOUNT];

        string[] operators = FindOperators(normalizedQuery);

        normalizedQuery = Tools.Normalize(query);

        this.QUERY_WORDS_AMOUNT = normalizedQuery.Count;

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.result[i] = (0d, i);
        }

        for (int i = 0; i < QUERY_WORDS_AMOUNT; i++)
        {
            if (Data.wordsIndex.ContainsKey(normalizedQuery[i]))
            {
                _wordIndex = Data.wordsIndex[normalizedQuery[i]];
                for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                {
                    this.result[j].Item1 += Data.tfIdf[_wordIndex, j];
                    if (operators[i].Contains("*"))
                    {
                        int _amount = operators[i].LastIndexOf("*") - operators[i].IndexOf("*") + 1;
                        this.result[j].Item1 *= Math.Pow(2.0, _amount);
                    }
                }

                suggestion += normalizedQuery[i] + " ";
            }
            else
            {
                closestWord = Tools.ClosestWord(normalizedQuery[i], Data);
                suggestion += closestWord + " ";
            }
        }

        for (int i = 0; i < QUERY_WORDS_AMOUNT; i++)
        {
            if (Data.wordsIndex.ContainsKey(normalizedQuery[i]))
            {
                _wordIndex = Data.wordsIndex[normalizedQuery[i]];
                for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                {
                    if ((operators[i] == "!" && Data.tf[_wordIndex][j] != 0) || (operators[i].Contains('^') && Data.tf[_wordIndex][j] == 0))
                    {
                        this.result[j].Item1 *= 0;
                    }
                }
                if (operators.Length > i + 1 && operators[i].Contains("~") && operators[i + 1].Contains("~"))
                {
                    (int, int, int)[] a = Tools.minDistance(Data.wordsIndex[normalizedQuery[i]], Data.wordsIndex[normalizedQuery[i + 1]], Data.wordPositionInText, DOCUMENTS_AMOUNT);
                    for (int j = 0; j < a.Length; j++)
                    {
                        this.result[a[j].Item3].Item1 += a[j].Item2 / a[j].Item1;
                    }
                }
            }
        }

        sortResult(DOCUMENTS_AMOUNT);
        resizeResult();
        /*
                for(int i = 0; i < DOCUMENTS_AMOUNT; i++){
                    System.Console.Write(i + " ");
                    for(int j = 0; j < Data.wordPositionInText[Data.wordsIndex[normalizedQuery[0]]][i].Count; j++){
                        System.Console.Write(Data.wordPositionInText[Data.wordsIndex[normalizedQuery[0]]][i][j] + " ");
                    }
                    System.Console.WriteLine();
                }

                for(int i = 0; i < DOCUMENTS_AMOUNT; i++){
                    System.Console.Write(i + " ");
                    for(int j = 0; j < Data.wordPositionInText[Data.wordsIndex[normalizedQuery[1]]][i].Count; j++){
                        System.Console.Write(Data.wordPositionInText[Data.wordsIndex[normalizedQuery[1]]][i][j] + " ");
                    }
                    System.Console.WriteLine();
                }
        */
        // (int, int)[] a = minDistance(Data.wordsIndex[normalizedQuery[0]], Data.wordsIndex[normalizedQuery[1]], Data.wordPositionInText, DOCUMENTS_AMOUNT);
        /*       for(int i = 0; i < a.Length; i++){
                   System.Console.WriteLine(a[i].Item1 + " " + a[i].Item2);
               }
          */
        for (int i = 0; i < this.result.Length; i++)
        {
            System.Console.Write(this.result[i]);
        }
        System.Console.WriteLine();
    }

    static string[] FindOperators(List<string> normalizedQuery)
    {
        int querySize = normalizedQuery.Count;
        string[] operators = new string[querySize];
        int k = 0;

        for (int i = 0; i < querySize; i++)
        {
            operators[i] = "";
        }

        for (int i = 0; i < querySize; i++)
        {
            for (int j = 0; j < normalizedQuery[i].Length; j++)
            {
                //System.Console.WriteLine(k+ " " + operators.Length);
                if (normalizedQuery[i].Length > 1)
                {
                    if (normalizedQuery[i][j] == '!')
                    {
                        operators[k] = "!";
                        continue;
                    }
                    if (normalizedQuery[i][j] == '*' && !operators[i].Contains('!'))
                    {
                        operators[k] += "*";
                        continue;
                    }
                    if (normalizedQuery[i][j] == '^' && !operators[i].Contains('!'))
                    {
                        operators[k] = "^" + operators[k];
                        continue;
                    }
                }
                else if (normalizedQuery[i][j] == '~')
                {
                    k--;
                    if (operators[k] != "!")
                    {
                        operators[k] = "~";
                        operators[k + 1] = "~";
                    }
                }
                else if (normalizedQuery[i][j] == '!' || normalizedQuery[i][j] == '^' || normalizedQuery[i][j] == '*')
                {
                    k--;
                }
            }
            k++;
        }
        string[] _operators = new string[k];
        for (int i = 0; i < k; i++)
        {
            _operators[i] = operators[i];
            System.Console.WriteLine(i + " " + operators[i]);
        }

        return _operators;

        //!**el **perro ! e^s e~l ^!papa ~ !de lo!s ^**cachorros
    }
    
    void sortResult(int DOCUMENTS_AMOUNT)
    {

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            for (int j = i + 1; j < DOCUMENTS_AMOUNT; j++)
            {
                if (this.result[i].Item1 < this.result[j].Item1)
                {
                    (double, int) _aux = this.result[i];
                    this.result[i] = this.result[j];
                    this.result[j] = _aux;
                }
            }
        }
    }

    void resizeResult()
    { //Optimizable con sortResult

        int newSize = 0;

        for (int i = 0; i < this.result.Length; i++)
        {
            if (this.result[i].Item1 == 0)
            {
                newSize = i;
                break;
            }
        }

        (double, int)[] _auxResult = new (double, int)[newSize];

        for (int i = 0; i < newSize; i++)
        {
            _auxResult[i] = this.result[i];
        }

        this.result = _auxResult;
    }
    /*
        static (bool[], bool[]) ApplyOperators((double, int)[] result, int[] operatorsPosition){
            for(int i = 0; i < operatorsPosition.Length; i++){
                if(operatorsPosition[i] == '0'){

                }
            }
        }*/

    /*
    foreach(KeyValuePair<string, int> a in Data.wordsIndex)
    {
        System.Console.Write(a.Key + "\t\t");   
        for(int j = 0; j < Data.tfIdf[a.Value].Count; j++)
        {

            System.Console.Write(Data.tfIdf[a.Value][j] + "\t");
        }
        System.Console.WriteLine();
    }*/


    /*
    static string Suggestion(string query, List<string> words){

        string[] queryWords = query.Split(" ");    
        string result = "";

        foreach(string queryWord in queryWords){

            string partialResult = "";
            int minDistance = int.MaxValue;

            foreach(string word in words){
                int distance = EditDistance(word, queryWord);
                if(distance < minDistance){
                    partialResult = word;
                    minDistance = distance;
                }
            }

            result += (partialResult + " ");
        }

        return result;
    }

    static List<string> ListWords(List<string> fileContent){

        List<string> words = new List<string>();

        foreach(string document in fileContent){
            //System.Console.WriteLine("ListWords: " + document);
            List<string> auxWords = Normalize(document);
            foreach(string word in auxWords){
                if(words.IndexOf(word) == -1){
                    //System.Console.WriteLine(word);
                    words.Add(word);
                }
            }
        }
        return words;
    }*/
}