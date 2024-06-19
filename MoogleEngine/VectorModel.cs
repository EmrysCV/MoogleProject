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

public class VectorModel
{
    public Dictionary<string, int> WordsIndex { get; private set; }
    public List<int[]> TF { get; private set; }
    public double[,] TFIDF { get; private set; }
    public List<List<int>[]> WordPositionInText { get; private set; }
    public Token[][] TextWordByWord { get; private set; }
    public int DOCUMENTS_AMOUNT { get; private set; }

    public VectorModel(Document[] Corpus)
    {
        this.DOCUMENTS_AMOUNT = Corpus.Length;
        this.WordsIndex = new Dictionary<string, int>();
        this.WordPositionInText = new List<List<int>[]>();
        this.TextWordByWord = new Token[this.DOCUMENTS_AMOUNT][];
        this.TF = new List<int[]>();
        CalcTF(Corpus);
        this.TFIDF = new double[this.TF.Count, this.DOCUMENTS_AMOUNT];
        CalcTFIDF();
    }

    void AddWord(string word, int wordIndex, int documentIndex, int wordPosition)
    {
        this.WordsIndex.Add(word, wordIndex);
        this.TF.Add(new int[this.DOCUMENTS_AMOUNT]);
        this.TF[wordIndex][documentIndex] = 1;
        this.WordPositionInText.Add(new List<int>[this.DOCUMENTS_AMOUNT]);

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.WordPositionInText[wordIndex][i] = new List<int>();
        }

        this.WordPositionInText[wordIndex][documentIndex].Add(wordPosition);
    }

    public void CalcTF(Document[] corpus)
    {
        int wordIndex = 0;
        int documentIndex = 0;
        int wordPosition = 1;

        foreach (Document document in corpus)
        {
            Token[] normalizedDocument = Tools.Parse(document.Text);
            TextWordByWord[documentIndex] = normalizedDocument;

            foreach (var word in normalizedDocument)
            {
                if (!this.WordsIndex.ContainsKey(word.Lexem))
                {
                    AddWord(word.Lexem, wordIndex, documentIndex, wordPosition);
                    wordIndex++;
                }
                else
                {
                    this.TF[this.WordsIndex[word.Lexem]][documentIndex]++; //Aumentar la frecuencia de la palabra
                    this.WordPositionInText[this.WordsIndex[word.Lexem]][documentIndex].Add(wordPosition); //Llenar la tabla con las posiciones de las palabras en los textos
                }
                wordPosition++;
            }
            documentIndex++;
            wordPosition = 1;
        }
    }

    double CalcIDF(int[] wordTf)
    {
        double wordIdf;
        int df = 0;

        for (int i = 0; i < wordTf.Length; i++)
        {
            if (wordTf[i] != 0) df++;
        }
        double logArgument = (double)this.DOCUMENTS_AMOUNT / df;
        wordIdf = Math.Log10(logArgument);

        return wordIdf;
    }

    public void CalcTFIDF()
    {
        double _wordIdf;

        for (int i = 0; i < this.TF.Count; i++)
        {
            _wordIdf = CalcIDF(this.TF[i]);
            for (int j = 0; j < this.DOCUMENTS_AMOUNT; j++)
            {
                this.TFIDF[i, j] = (this.TF[i][j] * _wordIdf);
            }
        }
    }
}


