using System.Collections.Immutable;

namespace AppToolkit.Html.Tokens
{
    class EndTagToken : TagToken
    {
        public override TokenType Type => TokenType.EndTag;

        public EndTagToken(string tagName, ImmutableArray<AttributeToken> attributes, bool isSelfClosing)
            : base(tagName, attributes, isSelfClosing)
        { }
    }
}
