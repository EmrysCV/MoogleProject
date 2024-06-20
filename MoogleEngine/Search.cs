using MoogleEngine.Tools;

namespace MoogleEngine;

class Search
{
    public int DOCUMENTS_AMOUNT { get; private set; }
    public string suggestion { get; private set; }
    public int queryWordsAmount { get; private set; }
    public (double, int)[] result { get; private set; }
    public List<string> normalizedQuery { get; private set; }

    public Search(string query, VectorModel Model, Dictionary<string, HashSet<string>> synonymsDictionary)
    {
        this.normalizedQuery = Preprocessing.Normalize(query, true);
        string[] operators = QueryTools.FindOperators(this.normalizedQuery);
        this.normalizedQuery = Preprocessing.Normalize(query);

        string closestWord;

        this.DOCUMENTS_AMOUNT = Model.DOCUMENTS_AMOUNT;
        this.suggestion = "";
        this.queryWordsAmount = this.normalizedQuery.Count;
        this.result = new (double, int)[DOCUMENTS_AMOUNT];


        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.result[i] = (0d, i);
        }

        for (int i = 0; i < queryWordsAmount; i++)
        {
            if (Model.WordsIndex.ContainsKey(this.normalizedQuery[i]))
            {
                ProcessQueryWord(Model, operators, this.normalizedQuery[i], i);
                suggestion += this.normalizedQuery[i] + " ";
            }
            else
            {
                closestWord = QueryTools.ClosestWord(this.normalizedQuery[i], Model.WordsIndex);
                suggestion += closestWord + " ";
            }
            if (synonymsDictionary.ContainsKey(this.normalizedQuery[i]))
            {
                foreach (string synonym in synonymsDictionary[this.normalizedQuery[i]])
                {
                    Console.WriteLine(synonym);
                    if (Model.WordsIndex.ContainsKey(synonym))
                    {
                        ProcessQueryWord(Model, operators, synonym, i, true);
                    }
                }
            }
        }

        ApplyOperators(Model, operators);

        this.result = Array.FindAll(this.result, (item) => item.Item1 != 0); 
        Array.Sort(this.result, (item1, item2) => item2.Item1.CompareTo(item1.Item1));

        for (int i = 0; i < this.result.Length; i++)
        {
            System.Console.Write(this.result[i]);
        }
        System.Console.WriteLine();
    }

    void ProcessQueryWord(VectorModel Model, string[] operators, string word, int i, bool isSynonym = false)
    {
        int _wordIndex = Model.WordsIndex[word];
        for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
        {
            if (operators[i].Contains("*"))
            {
                int _power = operators[i].LastIndexOf("*") - operators[i].IndexOf("*") + 1;
                this.result[j].Item1 += (isSynonym ? Model.TFIDF[_wordIndex, j] / 2 : Model.TFIDF[_wordIndex, j]) * Math.Pow(2.0, _power);
            }
            else
            {
                this.result[j].Item1 += (isSynonym ? Model.TFIDF[_wordIndex, j] / 2 : Model.TFIDF[_wordIndex, j]);
            }
        }
    }

    void ApplyOperators(VectorModel Model, string[] operators)
    {
        List<int[]> distancesList = new List<int[]>();
        int _wordIndex;

        for (int i = 0; i < this.queryWordsAmount; i++)
        {
            if (Model.WordsIndex.ContainsKey(this.normalizedQuery[i]))
            {
                _wordIndex = Model.WordsIndex[this.normalizedQuery[i]];
                if (operators[i] == "!" || operators[i].Contains('^'))
                {
                    for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                    {
                        if ((operators[i] == "!" && Model.TF[_wordIndex][j] != 0) || (operators[i].Contains('^') && Model.TF[_wordIndex][j] == 0))
                        {
                            this.result[j].Item1 *= 0;
                        }
                    }
                }
                if (operators.Length > i + 1 && operators[i].Contains("~") && operators[i + 1].Contains("~") && Model.WordsIndex.ContainsKey(this.normalizedQuery[i + 1]))
                {
                    distancesList.Add(QueryTools.MinDistance(Model.WordsIndex[this.normalizedQuery[i]], Model.WordsIndex[this.normalizedQuery[i + 1]], Model.WordPositionsInText, DOCUMENTS_AMOUNT));
                }
            }
        }

        double _maxScore = int.MinValue;

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++) if (this.result[i].Item1 > _maxScore) _maxScore = this.result[i].Item1;
         
        for (int i = 0; i < distancesList.Count; i++)
        {
            for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
            {
                if (this.result[j].Item1 != 0)
                {
                    this.result[j].Item1 += _maxScore / (double)distancesList[i][j];
                }
            }
        }
    }
}