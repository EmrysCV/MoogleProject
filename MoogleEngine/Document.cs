namespace MoogleEngine;

public class Document{
    public string Name {get; private set;}
    public string Text {get; private set;}

    public Document(string name, string text){
        this.Name = name;
        this.Text = text;
    }
}