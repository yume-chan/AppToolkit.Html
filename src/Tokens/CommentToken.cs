using System.Text;

namespace AppToolkit.Html.Tokens
{
    class CommentToken : Token
    {
        public override TokenType Type => TokenType.Comment;

        public StringBuilder Data { get; }

        public CommentToken() { Data = new StringBuilder(); }

        public CommentToken(string value) { Data = new StringBuilder(value); }
    }
}
