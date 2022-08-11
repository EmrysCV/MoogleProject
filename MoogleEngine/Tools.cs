/*
    En esta clase estan agrupados los metodos implementados para las distintas funcionalidades 
    requeridas para la implementacion del proyecto no el modelo de 
*/

namespace MoogleEngine;

class Tools
{
    public static int EditDistance(string wordA, string wordB)
    {

        int sizeA = wordA.Length;
        int sizeB = wordB.Length;

        int[,] dp = new int[sizeA + 1, sizeB + 1];

        for (int i = 0; i <= sizeA; i++)
        {
            dp[i, 0] = i;
        }
        for (int j = 0; j <= sizeB; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= sizeA; i++)
        {
            for (int j = 1; j <= sizeB; j++)
            {
                if (wordA[i - 1] == wordB[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1];
                }
                else
                {
                    dp[i, j] = 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
                }
            }
        }

        return dp[sizeA, sizeB];
    }

    public static List<string> Normalize(string text, bool isQuery = false)
    {
        //áéíóúü
        //System.Console.WriteLine("Normlizar: " + text);

        char[] spliters = { ' ', '\n', '\t', ',', '.', ':', ';' };
        string[] words = text.Split(spliters);
        string newWord;
        List<string> listWords = new List<string>();

        foreach (string word in words)
        {

            newWord = "";
            foreach (char c in word)
            {
                char _ac = char.ToLower(c);
                if (_ac == 'á') { newWord += "a"; continue; }
                if (_ac == 'é') { newWord += "e"; continue; }
                if (_ac == 'í') { newWord += "i"; continue; }
                if (_ac == 'ó') { newWord += "o"; continue; }
                if (_ac == 'ú') { newWord += "u"; continue; }
                if (_ac == 'ü') { newWord += "u"; continue; }
                //if(c == 'ñ'){ newWord += "ñ"; continue;}

                if (char.IsLetterOrDigit(_ac))
                {
                    newWord += _ac.ToString();
                    continue;
                }

                if (isQuery)
                {
                    if (_ac == '!') { newWord += _ac.ToString(); continue; }
                    if (_ac == '^') { newWord += _ac.ToString(); continue; }
                    if (_ac == '~') { newWord += _ac.ToString(); continue; }
                    if (_ac == '*') { newWord += _ac.ToString(); continue; }
                }
            }

            if (newWord != "") listWords.Add(newWord);
        }

        return listWords;
    }

    public static string ClosestWord(string word, TextProcess Data)
    {
        int minDistance = int.MaxValue;
        string result = "";

        foreach (KeyValuePair<string, int> element in Data.wordsIndex)
        {
            int distance = Tools.EditDistance(word, element.Key);
            if (distance < minDistance)
            {
                minDistance = distance;
                result = element.Key;
            }
        }
        return result;
    }

        //Modulo para determinar en cual documento las palabras estan mas cercanas
    public static (int, int, int)[] minDistance(int word1, int word2, List<List<int>[]> wordPositionInText, int DOCUMENTS_AMOUNT)
    {
        (int, int, int)[] minDistanceAndFrequency = new (int, int, int)[DOCUMENTS_AMOUNT];
        int minDistance = int.MaxValue;
        int frecuency = 0;
        int _distance;

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            // System.Console.WriteLine(wordPositionInText[word2][i].Count);
            for (int j = 0; j < wordPositionInText[word1][i].Count; j++)
            {
                int k = 0;
                while (k < wordPositionInText[word2][i].Count)
                {
                    _distance = wordPositionInText[word2][i][k] - wordPositionInText[word1][i][j];
                    //  System.Console.WriteLine(_distance);
                    if (_distance > 0)
                    {
                        if (_distance < minDistance)
                        {
                            minDistance = _distance;
                            frecuency = 1;
                        }
                        else if (_distance == minDistance)
                        {
                            frecuency++;
                        }
                        break;
                    }
                    k++;
                }
            }
            minDistanceAndFrequency[i] = (minDistance, frecuency, i);
            minDistance = int.MaxValue;
            frecuency = 0;
        }

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            for (int j = i + 1; j < DOCUMENTS_AMOUNT; j++)
            {
                if ((minDistanceAndFrequency[i].Item1 > minDistanceAndFrequency[j].Item1) || (minDistanceAndFrequency[i].Item1 == minDistanceAndFrequency[j].Item1 && minDistanceAndFrequency[i].Item2 < minDistanceAndFrequency[j].Item2))
                {
                    (int, int, int) _aux = minDistanceAndFrequency[i];
                    minDistanceAndFrequency[i] = minDistanceAndFrequency[j];
                    minDistanceAndFrequency[j] = _aux;
                }
            }
        }

        int newSize = 0;

        for (int i = 0; i < minDistanceAndFrequency.Length; i++)
        {
            if (minDistanceAndFrequency[i].Item2 == 0)
            {
                newSize = i;
                break;
            }
        }

        (int, int, int)[] _auxResult = new (int, int, int)[newSize];

        for (int i = 0; i < newSize; i++)
        {
            _auxResult[i] = minDistanceAndFrequency[i];
        }

        return _auxResult;
    }

    public static (List<string>, string[]) LoadDocuments()
    {
        string[] directory = Directory.GetFiles(Path.Join("..", "Content"));
        List<string> fileContent = new List<string>();

        foreach (string document in directory)
        {
            if (File.ReadAllText(document) != null)
            {
                fileContent.Add(File.ReadAllText(document));
            }
            else
            {
                fileContent.Add("");
            }
        }
        System.Console.WriteLine(directory.Length + " " + fileContent.Count);

        return (fileContent, directory);
    }
}

