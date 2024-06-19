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
        this.normalizedQuery = Tools.Normalize(query, true);
        string[] operators = Tools.FindOperators(this.normalizedQuery);
        this.normalizedQuery = Tools.Normalize(query);

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
            if (Model.wordsIndex.ContainsKey(this.normalizedQuery[i]))
            {
                ProcessQueryWord(Model, operators, this.normalizedQuery[i], i);
                suggestion += this.normalizedQuery[i] + " ";
            }
            else
            {
                closestWord = Tools.ClosestWord(this.normalizedQuery[i], Model.wordsIndex);
                suggestion += closestWord + " ";
            }
            if (synonymsDictionary.ContainsKey(this.normalizedQuery[i]))
            {
                foreach (string synonym in synonymsDictionary[this.normalizedQuery[i]])
                {
                    if (Model.wordsIndex.ContainsKey(synonym))
                    {
                        ProcessQueryWord(Model, operators, synonym, i, true);
                    }
                }
            }
        }

        ApplyOperators(Model, operators);
        SortResult();
        ResizeResult();

        for (int i = 0; i < this.result.Length; i++)
        {
            System.Console.Write(this.result[i]);
        }
        System.Console.WriteLine();
    }

    void ProcessQueryWord(VectorModel Model, string[] operators, string word, int i, bool isSynonym = false)
    {
        int _wordIndex = Model.wordsIndex[word];
        for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
        {
            if (operators[i].Contains("*"))
            {
                int _power = operators[i].LastIndexOf("*") - operators[i].IndexOf("*") + 1;
                this.result[j].Item1 += (isSynonym ? Model.tfIdf[_wordIndex, j] / 2 : Model.tfIdf[_wordIndex, j]) * Math.Pow(2.0, _power);
            }
            else
            {
                this.result[j].Item1 += (isSynonym ? Model.tfIdf[_wordIndex, j] / 2 : Model.tfIdf[_wordIndex, j]);
            }
        }
    }

    void ApplyOperators(VectorModel Model, string[] operators)
    {
        List<int[]> distancesList = new List<int[]>();
        double maxScore = (double)int.MinValue;
        int _wordIndex;

        for (int i = 0; i < this.queryWordsAmount; i++)
        {
            if (Model.wordsIndex.ContainsKey(this.normalizedQuery[i]))
            {
                _wordIndex = Model.wordsIndex[this.normalizedQuery[i]];
                if (operators[i] == "!" || operators[i].Contains('^'))
                {
                    for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                    {
                        if ((operators[i] == "!" && Model.tf[_wordIndex][j] != 0) || (operators[i].Contains('^') && Model.tf[_wordIndex][j] == 0))
                        {
                            this.result[j].Item1 *= 0;
                        }
                    }
                }
                if (operators.Length > i + 1 && operators[i].Contains("~") && operators[i + 1].Contains("~") && Model.wordsIndex.ContainsKey(this.normalizedQuery[i + 1]))
                {
                    distancesList.Add(Tools.MinDistance(Model.wordsIndex[this.normalizedQuery[i]], Model.wordsIndex[this.normalizedQuery[i + 1]], Model.wordPositionInText, DOCUMENTS_AMOUNT));
                }
            }
        }

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            if (this.result[i].Item1 > maxScore)
            {
                maxScore = this.result[i].Item1;
            }
        }

        for (int i = 0; i < distancesList.Count; i++)
        {
            for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
            {
                if (this.result[j].Item1 != 0)
                {
                    this.result[j].Item1 += maxScore / (double)distancesList[i][j];
                }
            }
        }
    }

    void SortResult()
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

    void ResizeResult()
    {
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
}