using System.Text.Json;

namespace MoogleEngine.Tools;

public static class Preprocessing
{
    /*
        Devuelve una lista con los contenidos de los documentos y un array con las direcciones relativas de cada documento
    */
    public static Document[] LoadDocuments()
    {
        string[] directory = Directory.GetFiles(Path.Join("..", "Content"));
        List<Document> corpus = new();

        foreach (string document in directory)
        {
            char[] splitters = { '/', '\\' };
            string documentText = File.ReadAllText(document);

            if (documentText != null)
            {
                string _name = document.Split(splitters).Last(); //Get the file's name with extension.

                corpus.Add(new(
                    _name.Substring(0, _name.LastIndexOf('.')), //Get the file's name without extension.
                    documentText,
                    Parse(documentText)
                ));
            }
        }

        System.Console.WriteLine(directory.Length + " " + corpus.Count);

        return corpus.ToArray();
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

    /*
        Recive como parametros el texto que se va a normalizar(eliminar todos los caracteres que no sean digitos, letras o espacios,
      ademas de llevar todo a minusculas) y una variable indicando si el texto es una query o no(por defecto en falso)
        Devuelve una lista donde en cada posicion continene una de las palabras, del texto recivido como parametro, ya normalizada
    */

    //TODO ver que hago con esto
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
}