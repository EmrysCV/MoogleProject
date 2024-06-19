using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MoogleEngine;
using System.Diagnostics;
using System.Security.Principal;

public class Program
{
    public static VectorModel Model { get; private set; }
    public static (List<string>, string[]) Corpus { get; private set; }
    public static Dictionary<string, HashSet<string>> SynonymsDictionary { get; private set; }

    public static void Main(string[] args)
    {
        Stopwatch crono = new Stopwatch();
        crono.Start();

        Corpus = Tools.LoadDocuments();
        SynonymsDictionary = Tools.LoadAndCreateSynonymsDictionary();
        Model = new VectorModel(Corpus.Item1);

        crono.Stop();
        System.Console.WriteLine((double)crono.ElapsedMilliseconds / 1000);

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }


        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}