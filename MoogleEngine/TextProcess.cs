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
class TextProcess
{
    public Dictionary<string, int> wordsIndex;
    public List<int[]> tf;
    public double[,] tfIdf;
    public int DOCUMENTS_AMOUNT;

    public TextProcess(List<string> documents)
    {
        DOCUMENTS_AMOUNT = documents.Count;
        wordsIndex = new Dictionary<string, int>();
        tf = new List<int[]>();
        CalcTf(documents);
        tfIdf = new double[this.tf.Count,DOCUMENTS_AMOUNT];
        CalcTfIdf();
    }

    void AddWord(string word, int wordIndex, int documentIndex)
    {   
        this.wordsIndex.Add(word, wordIndex);
        this.tf.Add(new int[this.DOCUMENTS_AMOUNT]);

        tf[wordIndex][documentIndex] = 1;
    }
    
    public void CalcTf(List<string> documents)
    {
        int _wordIndex = 0;
        int _documentIndex = 0;
        
        foreach (string document in documents)
        {
            List<string> normalizedDocument = Tools.Normalize(document);
            foreach (string word in normalizedDocument)
            {
                if (!wordsIndex.ContainsKey(word))
                {
                    AddWord(word, _wordIndex, _documentIndex);
                    _wordIndex++;
                }
                else
                {
                    tf[wordsIndex[word]][_documentIndex]++;
                }
            }
            _documentIndex++;
        }
    }
    
    //double CalcIdf(List<int> wordTf, int DOCUMENTS_AMOUNT)
    double CalcIdf(int[] wordTf)
    {
        double wordIdf;
        int df = 0;

        for(int i = 0 ; i < wordTf.Length; i++)
        {
            if(wordTf[i] != 0) df++;
        }
        double logArgument = (double)df/DOCUMENTS_AMOUNT;
        wordIdf = Math.Log10(1 + logArgument);

        return wordIdf;
    }

    public void CalcTfIdf()
    {    
        double _wordIdf;
        //int DOCUMENTS_AMOUNT = tf[0].Count; // sobra pude ser global

        for(int i = 0; i < this.tf.Count; i++)
        {
//            _wordIdf = CalcIdf(this.tf[i], DOCUMENTS_AMOUNT);
//            tfIdf.Add(new List<double>());
            _wordIdf = CalcIdf(this.tf[i]);
            for(int j = 0; j < DOCUMENTS_AMOUNT; j++)
            {
//                tfIdf[i].Add(tf[i][j] * _wordIdf);
                tfIdf[i,j] = (this.tf[i][j] * _wordIdf);
            }
        }
    }
}


