namespace MoogleEngine;

class Search // Cambiar los modificardores de acceso
{
    public string suggestion;
    public int QUERY_WORDS_AMOUNT;
    public (double, int)[] result;
    public List<string> normalizedQuery;

    public Search(string query, TextProcess Data)
    {
        List<int[]> distancesList = new List<int[]>();
        this.normalizedQuery = Tools.Normalize(query, true);
        string[] operators = FindOperators(this.normalizedQuery);
        this.normalizedQuery = Tools.Normalize(query);

        int DOCUMENTS_AMOUNT = Data.DOCUMENTS_AMOUNT;
        int _wordIndex;
        double maxScore = (double)int.MinValue;
        string closestWord;

        this.suggestion = "";
        this.QUERY_WORDS_AMOUNT = this.normalizedQuery.Count;
        this.result = new (double, int)[DOCUMENTS_AMOUNT];


        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.result[i] = (0d, i);
        }

        for (int i = 0; i < QUERY_WORDS_AMOUNT; i++)
        {
            if (Data.wordsIndex.ContainsKey(this.normalizedQuery[i]))
            {
                _wordIndex = Data.wordsIndex[this.normalizedQuery[i]];
                for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                {
                    this.result[j].Item1 += Data.tfIdf[_wordIndex, j];
                    if (operators[i].Contains("*"))
                    {
                        int _power = operators[i].LastIndexOf("*") - operators[i].IndexOf("*") + 1;
                        this.result[j].Item1 *= Math.Pow(2.0, _power);// no es potencia
                    }
                }

                suggestion += this.normalizedQuery[i] + " ";
            }
            else
            {
                closestWord = Tools.ClosestWord(this.normalizedQuery[i], Data);
                suggestion += closestWord + " ";
            }
        }

        for (int i = 0; i < QUERY_WORDS_AMOUNT; i++)
        {
            if (Data.wordsIndex.ContainsKey(this.normalizedQuery[i]))
            {
                _wordIndex = Data.wordsIndex[this.normalizedQuery[i]];
                if (operators[i] == "!" || operators[i].Contains('^'))
                {
                    for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                    {
                        if ((operators[i] == "!" && Data.tf[_wordIndex][j] != 0) || (operators[i].Contains('^') && Data.tf[_wordIndex][j] == 0))
                        {
                            this.result[j].Item1 *= 0;
                        }
                    }
                }
                if (operators.Length > i + 1 && operators[i].Contains("~") && operators[i + 1].Contains("~") && Data.wordsIndex.ContainsKey(this.normalizedQuery[i + 1]))
                {
                    distancesList.Add(Tools.minDistance(Data.wordsIndex[this.normalizedQuery[i]], Data.wordsIndex[this.normalizedQuery[i + 1]], Data.wordPositionInText, DOCUMENTS_AMOUNT));
                    for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                    {
                        System.Console.WriteLine(distancesList[distancesList.Count - 1][j] + " " + j);
                    }
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

        sortResult(DOCUMENTS_AMOUNT);
        resizeResult();

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
        int _k, _m;
        bool flag = false;

        _m = _k = 0;

        for (int i = 0; i < querySize; i++)
        {
            operators[i] = "";
        }

        for (int i = 0; i < querySize; i++)
        {
            for (int j = 0; j < normalizedQuery[i].Length; j++)
            {
                switch (normalizedQuery[i][j])
                {
                    case '!':
                        _m++;
                        operators[_k] = "!";
                        break;
                    case '*':
                        _m++;
                        if (!operators[_k].Contains('!'))
                        {
                            operators[_k] += "*";
                        }
                        break;
                    case '^':
                        _m++;
                        if (!operators[_k].Contains('!'))
                        {
                            operators[_k] = "^" + operators[_k];
                        }
                        break;
                    case '~':
                        _m++;
                        flag = true;
                        break;
                }
            }

            if (_m == normalizedQuery[i].Length)
            {
                operators[_k] = "";
                _k--;
                if (operators[_k] != "!" && flag)
                {
                    operators[_k] += "~";
                    operators[_k + 1] = "~";
                    _m++;
                }
            }

            _k++;
            _m = 0;
            flag = false;
        }

        string[] _operators = new string[_k];

        for (int i = 0; i < querySize; i++)
        {
            System.Console.WriteLine(i + " " + operators[i]);
        }

        for (int i = 0; i < _k; i++)
        {
            _operators[i] = operators[i];
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
}