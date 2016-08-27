using System.Text;

namespace AppToolkit.Html.Tokens
{
    internal class CommentToken : Token
    {
        public override TokenType Type => TokenType.Comment;

        public string Data { get; }

        public CommentToken(string value) { Data = value; }
    }

    internal class CommentTokenBuilder
    {
        public StringBuilder Data { get; } = new StringBuilder();

        public CommentToken Create()
        {
            var result = new CommentToken(Data.ToString());

            Data.Clear();

            return result;
        }
    }
}
