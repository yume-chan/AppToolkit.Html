namespace AppToolkit.Html.Tokens
{
    class CharacterToken : Token
    {
        public override TokenType Type => TokenType.Character;

        public char Data { get; }

        public CharacterToken(char data)
        {
            Data = data;
        }
    }
}
