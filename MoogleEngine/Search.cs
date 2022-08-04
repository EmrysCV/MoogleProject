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

class Search // Cambiar los modificardores de acceso
{
    public string query;
    public string suggestion;
    public int QUERY_WORDS_AMOUNT;
    public (double , int)[] result;

    public Search(string query, TextProcess Data)
    {/*
        char[] spliters = { ' ', '\n', '\t', ',', '.', ':', ';' };
        string[] normalizedQuery = query.Split(spliters);*/
        this.query = query;
        List<string> normalizedQuery = Tools.Normalize(query, true);

        //int DOCUMENTS_AMOUNT = Data.tfIdf[0].Count;
        int DOCUMENTS_AMOUNT = Data.DOCUMENTS_AMOUNT;
        this.QUERY_WORDS_AMOUNT = normalizedQuery.Count;
        suggestion = "";
        string closestWord;
        int _wordIndex;
        bool[] shouldContain;
        bool[] shouldNotContain;

        this.result = new (double, int)[DOCUMENTS_AMOUNT];

        int[] operatorsPosition = FindOperators(normalizedQuery, Data);

        normalizedQuery = Tools.Normalize(query);

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.result[i] = (0d, i);
        }

        for (int i = 0; i < QUERY_WORDS_AMOUNT; i++)
        {
            if (Data.wordsIndex.ContainsKey(normalizedQuery[i]))
            {
                _wordIndex = Data.wordsIndex[normalizedQuery[i]];
                if(operatorsPosition[i] == 0){

                }
                else{
                    for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                    {
                        this.result[j].Item1 += Data.tfIdf[_wordIndex,j];
                        /*if(operatorsPosition[i] > 1 && Data.tfIdf[_wordIndex][j] != 0) {
                        result[j].Item1 *= operatorsPosition[i];
                        }*/
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

        sortResult(DOCUMENTS_AMOUNT);

        resizeResult();
      
        for(int i = 0; i < this.result.Length; i++){
            System.Console.Write(this.result[i]);
        }
        System.Console.WriteLine();
    }

    void sortResult(int DOCUMENTS_AMOUNT){

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

    void resizeResult(){ //Optimizable con sortResult
        
        int newSize = 0;
        
        for(int i = 0; i < this.result.Length; i++){
            if(this.result[i].Item1 == 0){
                newSize = i;
                break;
            }
        }
        
        (double, int)[] _auxResult = new (double, int)[newSize];

        for(int i = 0; i < newSize; i++){
            _auxResult[i] = this.result[i];
        }

        this.result =  _auxResult;
    }

    static int[] FindOperators(List<string> normalizedQuery ,TextProcess Data){
        
        int querySize = normalizedQuery.Count;
        int[] operators = new int[querySize];
        bool[] souldContain = new bool[querySize];
        bool[] sholdNotContain = new bool[querySize];

        for(int i = 0; i < querySize; i++){
            operators[i] = 1;
        }

        for(int i = 0; i < querySize; i++){
            for(int j = 0; j < normalizedQuery[i].Length; j++){
                if(normalizedQuery[i][j] == '!'){
                    
                    continue;
                }
                if(normalizedQuery[i][j] == '*'){
                    operators[i] *= 2;
                    continue;
                }
                if(normalizedQuery[i][j] == '^'){
                    if(operators[i] == 0) operators[i] = -1;
                    else operators[i] *= -1;
                    continue;
                }
                if(normalizedQuery[i][j] == '~'){

                }
            }
        }

        for(int i = 0; i < querySize; i++){
            System.Console.WriteLine(operators[i]);
        }
        return operators;
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