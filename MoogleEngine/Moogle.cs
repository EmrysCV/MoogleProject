using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static Dictionary<string, SearchResult> memory = new Dictionary<string, SearchResult>();

    public static SearchResult Query(string query, VectorModel model, Document[] corpus, Dictionary<string, HashSet<string>> synonymsDictionary)
    {
        Stopwatch cronos = new Stopwatch();
        cronos.Start();

        if (memory.ContainsKey(query))
        {
            cronos.Stop();
            System.Console.WriteLine((double)cronos.ElapsedMilliseconds / 1000);
            System.Console.WriteLine(model.TF.Count);
            return memory[query];
        }

        char[] spliters = { '\\', '/', '-' };

        Search result = new Search(query, model, synonymsDictionary);
        SearchItem[] items;

        int resultSize = result.result.Length;

        if (resultSize == 0)
        {
            items = new SearchItem[1] {
                new SearchItem("No hay coincidencias", "", 0f)
            };
        }
        else
        {
            items = new SearchItem[resultSize];
            for (int i = 0; i < resultSize; i++)
            {
                int _documentIndex = result.result[i].Item2;

                items[i] = new SearchItem(corpus[_documentIndex].Name, Tools.FindSnippet(_documentIndex, model, corpus, result.normalizedQuery), (float)(result.result[i].Item1));
            }
        }

        cronos.Stop();
        System.Console.WriteLine((double)cronos.ElapsedMilliseconds / 1000);
        System.Console.WriteLine(model.TF.Count);

        memory.Add(query, new SearchResult(items, result.suggestion));

        return new SearchResult(items, result.suggestion);
    }
}
