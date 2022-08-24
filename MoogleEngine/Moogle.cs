using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query, TextProcess Data, (List<string>, string[]) content)
    {

        Stopwatch cronos = new Stopwatch();
        cronos.Start();

        char[] spliters = { '\\', '/', '-' };

        Search result = new Search(query, Data);
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
                int _snippetSize = content.Item1[_documentIndex].Length >= 400 ? 400 : content.Item1[_documentIndex].Length;
                string[] _auxResult = content.Item2[_documentIndex].Split(spliters);

                items[i] = new SearchItem(_auxResult[_auxResult.Length - 1], content.Item1[_documentIndex].Substring(0, _snippetSize), (float)(result.result[i].Item1));
            }
        }

        cronos.Stop();
        System.Console.WriteLine((double)cronos.ElapsedMilliseconds / 1000);
        System.Console.WriteLine(Data.tf.Count);

        return new SearchResult(items, result.suggestion);
    }
}
