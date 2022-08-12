/*
    Implementacion del modelo de espacio vectorial:
       -Cada palabra es representada como un vector donde la componente i-esima es su valor de
        TfIdf para el documento i-esimo
       -Metodos implementados:
            -Constructor:
            -CalcTf: Recorro todos los documentos del directorio y les aplico el metodo Normalize a cada uno y obtengo
                     una lista con todas las palabras del documento y luego cuento cuantas veces aparece cada una en cada
                     documento y esta informacion la almaceno en una lista de arrays(utilizado como matriz)
                     que se llama tf(actua como matriz)
            -AddWord: Este Metodo es llamado cuando encuentro una palabra nueva que no aparece en mi diccrionario de
                      palabras wordIndex y este le asocia a la palbra su fila de la matriz

*/
namespace MoogleEngine;

public class TextProcess
{
    public Dictionary<string, int> wordsIndex;
    public List<int[]> tf;
    public double[,] tfIdf;
    public List<List<int>[]> wordPositionInText;
    public int DOCUMENTS_AMOUNT;

    public TextProcess(List<string> documents)
    {
        this.DOCUMENTS_AMOUNT = documents.Count;
        this.wordsIndex = new Dictionary<string, int>();
        this.wordPositionInText = new List<List<int>[]>();
        this.tf = new List<int[]>();
        CalcTf(documents);
        this.tfIdf = new double[this.tf.Count, this.DOCUMENTS_AMOUNT];
        CalcTfIdf();
    }

    void AddWord(string word, int wordIndex, int documentIndex, int wordPosition)
    {
        this.wordsIndex.Add(word, wordIndex);
        this.tf.Add(new int[this.DOCUMENTS_AMOUNT]);
        this.tf[wordIndex][documentIndex] = 1;

        this.wordPositionInText.Add(new List<int>[this.DOCUMENTS_AMOUNT]);
        this.wordPositionInText[wordIndex][documentIndex] = new List<int>();

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.wordPositionInText[wordIndex][i] = new List<int>();
        }

        this.wordPositionInText[wordIndex][documentIndex].Add(wordPosition);
    }

    public void CalcTf(List<string> documents)
    {
        int wordIndex = 0;
        int documentIndex = 0;
        int wordPosition = 1;

        foreach (string document in documents)
        {

            List<string> normalizedDocument = Tools.Normalize(document);
            foreach (string word in normalizedDocument)
            {
                if (!this.wordsIndex.ContainsKey(word))
                {
                    AddWord(word, wordIndex, documentIndex, wordPosition);
                    wordIndex++;
                }
                else
                {
                    this.tf[this.wordsIndex[word]][documentIndex]++; //Aumentar la frecuencia de la palabra
                    this.wordPositionInText[this.wordsIndex[word]][documentIndex].Add(wordPosition); //Llenar la tabla con las posiciones de las palabras en los textos
                }
                wordPosition++;
            }
            documentIndex++;
            wordPosition = 1;
        }
    }

    double CalcIdf(int[] wordTf)
    {
        double wordIdf;
        int df = 0;

        for (int i = 0; i < wordTf.Length; i++)
        {
            if (wordTf[i] != 0) df++;
        }
        double logArgument = (double)df / this.DOCUMENTS_AMOUNT;
        wordIdf = Math.Log10(1 + logArgument);

        return wordIdf;
    }

    public void CalcTfIdf()
    {
        double _wordIdf;

        for (int i = 0; i < this.tf.Count; i++)
        {
            _wordIdf = CalcIdf(this.tf[i]);
            for (int j = 0; j < this.DOCUMENTS_AMOUNT; j++)
            {
                this.tfIdf[i, j] = (this.tf[i][j] * _wordIdf);
            }
        }
    }
}


