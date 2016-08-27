using System.Collections.Immutable;

namespace AppToolkit.Html.Tokens
{
    class StartTagToken : TagToken
    {
        public override TokenType Type => TokenType.StartTag;

        public StartTagToken(string tagName, ImmutableArray<AttributeToken> attributes, bool isSelfClosing)
            : base(tagName, attributes, isSelfClosing)
        { }
    }
}
