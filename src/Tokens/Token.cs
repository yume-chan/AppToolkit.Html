namespace AppToolkit.Html.Tokens
{
    enum TokenType
    {
        Character,
        StartTag,
        EndTag,
        Comment,
        Doctype,
        EndOfFile,
        Text
    }

    abstract class Token
    {
        public abstract TokenType Type { get; }
    }
}
