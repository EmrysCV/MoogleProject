using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace MoogleEngine;

public static class Tools
{
    /*
        Recive como parametros dos palabras las cuales se va a calcular la distancia lexicografica(cuantas letras hay que cambiar, añadir o
      eliminar para que sean iguales)
        Devuelve la distancia lexicografica entre las dos palabras
    */
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

    /*
        Recive como parametros el texto que se va a normalizar(eliminar todos los caracteres que no sean digitos, letras o espacios,
      ademas de llevar todo a minusculas) y una variable indicando si el texto es una query o no(por defecto en falso)
        Devuelve una lista donde en cada posicion continene una de las palabras, del texto recivido como parametro, ya normalizada
    */
    public static List<string> Normalize(string text, bool isQuery = false)
    {
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

    public static Token[] Parse(string text)
    {
        List<Token> tokens = new List<Token>();
        string _word = "";
        int startingPosition = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsLetterOrDigit(text[i]))
            {
                _word += char.ToLower(text[i]);
            }
            else
            {
                if (_word != "")
                {
                    Token _token = new(_word, startingPosition);
                    tokens.Add(_token);

                    _word = "";
                }

                startingPosition = i + 1;
            }
        }


        if (_word != "")
        {
            Token _token = new(_word, startingPosition);
            tokens.Add(_token);
        }
        return tokens.ToArray();
    }

    /*
        Recive como parametros la palabra de cual vamos a obtener la que mas se parce en nuestro diccionario y el diccionario 
    donde esta guardado todo nuestro universo de palabras
        Retorna la palabra que mas se parece a la palabra recivida como parametro
    */

    public static string ClosestWord(string queryWord, Dictionary<string, int> wordsIndex)
    {
        int minDistance = int.MaxValue;
        string result = "";

        foreach (string word in wordsIndex.Keys)
        {
            int distance = Tools.EditDistance(queryWord, word);
            if (distance < minDistance)
            {
                minDistance = distance;
                result = word;
            }
        }
        return result;
    }

    /*
        Recive como parametros las dos palabras para las cuales se va comprobar cual es la menor distancia entre ellas en cada texto, 
      la estructura de datos en la cual estan almacenadas las posiciones de cada palabra en cada texto y la cantidad de documentos 
      de nuestro universo de documentos.
        Devuelve un array de enteros con la distancia minima entre estas dos palabras en cada documento.
    */

    public static int[] MinDistance(int word1, int word2, List<List<int>[]> wordPositionsInText, int DOCUMENTS_AMOUNT)
    {
        int[] minDistancePerDocument = new int[DOCUMENTS_AMOUNT];
        int minDistance = int.MaxValue;
        int j, k, _distance, _wordPosition;

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            j = k = 0;
            while (j < wordPositionsInText[word1][i].Count && k < wordPositionsInText[word2][i].Count)
            {
                _wordPosition = wordPositionsInText[word2][i][k];
                while (j < wordPositionsInText[word1][i].Count && wordPositionsInText[word1][i][j] < _wordPosition)
                {
                    _distance = _wordPosition - wordPositionsInText[word1][i][j];
                    if (_distance < minDistance)
                    {
                        minDistance = _distance;
                    }
                    j++;
                }

                if (j < wordPositionsInText[word1][i].Count)
                {
                    _wordPosition = wordPositionsInText[word1][i][j];
                }
                else
                {
                    break;
                }

                while (k < wordPositionsInText[word2][i].Count && wordPositionsInText[word2][i][k] < _wordPosition)
                {
                    _distance = _wordPosition - wordPositionsInText[word2][i][k];
                    if (_distance < minDistance)
                    {
                        minDistance = _distance;
                    }
                    k++;
                }
            }
            minDistancePerDocument[i] = minDistance;
            minDistance = int.MaxValue;
        }

        return minDistancePerDocument;
    }

    /*
        Recive como parametros el indice asignado al documento en el cual se busca el snippet, la estructura de datos que contiene todos los datos
      extraidos del preprocesamiento y la query normalizada
        Devuelve una cadena de texto con el snippet que mas importante para la query que se hizo
    */

    public static string FindSnippet(int documentIndex, TextProcessor Data, List<string> normalizedQuery)
    {
        int maxScoreI = 0;
        double maxScore = 0d;
        double _score = 0d;
        string snippet = "";
        Token[] document = Data.textWordByWord[documentIndex];

        for (int i = 0; i < 100 && i < document.Length; i++)
        {
            for (int j = 0; j < normalizedQuery.Count; j++)
            {
                if (document[i].Lexem == normalizedQuery[j])
                {
                    _score += Data.tfIdf[Data.wordsIndex[normalizedQuery[j]], documentIndex];
                    break;
                }
            }
        }

        maxScore = _score;

        for (int i = 100; i < document.Length; i++)
        {
            for (int j = 0; j < normalizedQuery.Count; j++)
            {
                if (document[i].Lexem == normalizedQuery[j])
                {
                    _score += Data.tfIdf[Data.wordsIndex[normalizedQuery[j]], documentIndex];
                }
                if (document[i - 100].Lexem == normalizedQuery[j])
                {
                    _score -= Data.tfIdf[Data.wordsIndex[normalizedQuery[j]], documentIndex];
                }
            }
            if (_score > maxScore)
            {
                maxScore = _score;
                maxScoreI = i - 99;
            }
        }

        int maxScoreEndI = 0;

        for (int i = maxScoreI; i < maxScoreI + 100 && i < document.Length; i++)
        {
            maxScoreEndI = i;
        }

        return Data.documents[documentIndex].Substring(document[maxScoreI].Position, document[maxScoreEndI].Position + document[maxScoreEndI].Lexem.Length - document[maxScoreI].Position);
    }

    /*
        Recive como parametro la query normalizada sin quitarle los operadores
        Devuelve en una array los operadores asociados a cada palabra
    */
    public static string[] FindOperators(List<string> normalizedQuery)
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

    /*
        Devuelve una lista con los contenidos de los documentos y un array con las direcciones relativas de cada documento
    */
    public static (List<string>, string[]) LoadDocuments()
    {
        string[] directory = Directory.GetFiles(Path.Join("..", "Content"));
        List<string> fileContent = new List<string>();

        foreach (string document in directory)
        {
            string documentBody = File.ReadAllText(document);

            fileContent.Add(documentBody != null ? documentBody : "");
        }

        System.Console.WriteLine(directory.Length + " " + fileContent.Count);

        return (fileContent, directory);
    }

    /*
        Devuelve un diccionario que relaciona cada palabra con el conjunto de todos sus posibles sinonimos
    */
    public static Dictionary<string, HashSet<string>> LoadAndCreateSynonymsDictionary()
    {
        Dictionary<string, HashSet<string>> synonymsDictionary = new Dictionary<string, HashSet<string>>();
        string json = File.ReadAllText(Path.Join("..", "sinonimos.json"));
        _Sinonyms deserializedJson = JsonSerializer.Deserialize<_Sinonyms>(json);

        for (int i = 0; i < deserializedJson.words.Length; i++)
        {
            for (int j = 0; j < deserializedJson.words[i].Length; j++)
            {
                if (!synonymsDictionary.ContainsKey(deserializedJson.words[i][j]))
                {
                    synonymsDictionary.Add(deserializedJson.words[i][j], new HashSet<string>());
                }
                for (int k = 0; k < deserializedJson.words[i].Length; k++)
                {
                    if (k != j)
                    {
                        synonymsDictionary[deserializedJson.words[i][j]].Add(Normalize(deserializedJson.words[i][k])[0]);
                    }
                }
            }
        }

        return synonymsDictionary;
    }
}

public class _Sinonyms
{
    public string[][] words { get; set; }
}