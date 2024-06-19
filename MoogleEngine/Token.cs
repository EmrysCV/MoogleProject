namespace MoogleEngine;

public class Token{
    public string Lexem {get; private set;}
    public int Position {get; private set;}

    public Token(string lex, int position){
        this.Lexem = lex;
        this.Position = position;
    }
}