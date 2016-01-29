namespace AppToolkit.Html.Tokens
{
    class TextToken : Token
    {
        public string Data { get; }

        public override TokenType Type => TokenType.Text;

        public TextToken(string data)
        {
            Data = data;
        }
    }
}
