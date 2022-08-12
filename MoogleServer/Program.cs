using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MoogleEngine;

public class Program
{
    public static TextProcess Data;
    public static (List<string>, string[]) content;

    public static void Main(string[] args)
    {

        content = Tools.LoadDocuments();
        Data = new TextProcess(content.Item1);
        
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