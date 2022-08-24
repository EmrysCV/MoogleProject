using System.IO;
using System.Text;
/*
    En esta clase estan agrupados los metodos implementados para las distintas funcionalidades 
    requeridas para la implementacion del proyecto  
*/

namespace MoogleEngine;

public static class Tools
{
    /*
    Metodo para determinar cuan parecidas son dos palabras lexicograficamente osea cuantas modificacion del tipo cambio,
    eliminacion o agrego de letra hay q hacer para transformar una en la otra (utilizado en para obtener la sugerencia de
    busqueda):
        -Para llevar a cabo el calculo fue usado el algoritmo de Distancia de Levinstein
        -Por ultimo el metodo devuelve un numero entero q es la distancia entre ambas palabras
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
    Metodo para normalizar los documentos osea eliminar todos los elementos y caracteres que no me interesan:
        -Primero con el metodo Split() para quedarme con las palabras se supone que los textos estan bien escritos pero
        para mas precision en vez de seprar solo por espacios utilizo tambien los caracteres de puntuacion usuales .,;: 
        y los caracteres especiales \n y \t con el objetivo de evitar errores como (coimdas:fideos, arroz)
        -Se procesa cada palabra caracter por caracter, los vuelvo a minusculas con el metodo char.ToLower() y solo me 
        quedo con los q sean letras y numeros usando elmetodo char.IsLetterorDigit() ademas de eliminar las letras con
        tilde y diericis del alfabeto latino
        -Si no se obtiene un string vacio pues lo agrego a la lista de string que es devuelto con las palabras procesadas
        -Este metodo ademas cuando e lparametro isQuery esta activo tambien deja los caracteres correspondites a los
        operadores ~ * ! ^
        -Por ultimo el metodo de vuvelve la lista con las palabras ya procesadas
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
                if (_ac == 'á') { newWord += "a"; continue; }
                if (_ac == 'é') { newWord += "e"; continue; }
                if (_ac == 'í') { newWord += "i"; continue; }
                if (_ac == 'ó') { newWord += "o"; continue; }
                if (_ac == 'ú') { newWord += "u"; continue; }
                if (_ac == 'ü') { newWord += "u"; continue; }
                //if(_ac == 'ñ'){ newWord += "ñ"; continue;}

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
    /*
    Metodo para encontrar la palabra mas lexicograficamente parecida a otra
        -Busca en el diccionario de todas las palabras que aprecen en los documentos de Content y devuelve la que tenga
        menor distancia de Levinstein con esta
        -Por ultimo retorna la palabra
    */
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

    /* 
    Metodo para determinar cuan cercanas estan dos palbaras en cada documento
        -En la estructura de datos wordPositionsInText estan guardadas las posiciones de cada palabra en cada documento
        -Entonces itera por los indices de los documentos y en cada documento determina que tan cerca estan las palabras
        - para esto iteramos las posisiones de la primera palabra y con la posicion fijada entonces rcorremos con el while las pocisiones de la segunda palabra, determinando la distancia minima entre ambas
    */
    public static int[] minDistance(int word1, int word2, List<List<int>[]> wordPositionsInText, int DOCUMENTS_AMOUNT)
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
    Metodo para leer los archivos de texto en la carpeta Content:
        -El Metodo Directory.GetFiles() devuevle un array de strings con las direcciones de los archivos y carpetas 
        contenidos en la carpeta culla direccion se le pase por parametro
        -El Metodo File.ReadAllText() Devuelve el un string con el contenido de una archivo de texto culla direccion 
        se le pasa por parameto
        -Por ultimo el metodo retorna tanto el array de string directory con las direcciones de todos los archivos y 
        carpetas dentro de Content y una lista de strings con el contenido de los archivos
    */
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