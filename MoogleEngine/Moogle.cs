using System.IO;
using System.Text;
using System.Diagnostics;

namespace MoogleEngine;

public static class Moogle
{
    public static SearchResult Query(string query, TextProcess Data, (List<string>, string[]) content){
       
        Stopwatch cronos = new Stopwatch();
        cronos.Start();
        /*(List<string>, string[]) content = Tools.LoadDocuments();
        TextProcess Data = new TextProcess(content.Item1);        
        */
        char[] spliters = {'\\' , '/' , '-'}; 

        Search result = new Search(query, Data);

        int resultSize = result.result.Length; 

        SearchItem[] items;

        if(resultSize == 0)
        {
            items = new SearchItem[1] {
                new SearchItem("No hay coincidencias", "", 0f)
            };
        }
        else
        {
            items = new SearchItem[resultSize];
            for(int i = 0; i < resultSize; i++)
            {
                int _documentIndex = result.result[i].Item2;

                string[] _auxResult = content.Item2[_documentIndex].Split(spliters);
                
                if(content.Item1[_documentIndex].Length >= 400){
                    items[i] = new SearchItem(_auxResult[_auxResult.Length-1], content.Item1[_documentIndex].Substring(0, 400), (float)(result.result[i].Item1));
                }
                else{
                    items[i] = new SearchItem(_auxResult[_auxResult.Length-1], content.Item1[_documentIndex].Substring(0, content.Item1[_documentIndex].Length), (float)(result.result[i].Item1));
                }
            }
        }

        cronos.Stop();
        System.Console.WriteLine((double)cronos.ElapsedMilliseconds/1000);
        System.Console.WriteLine(Data.tf.Count);
        
        return new SearchResult(items, result.suggestion);
    }
}
