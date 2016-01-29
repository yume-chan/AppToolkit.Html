using System.Collections.Generic;
using System.Text;

namespace AppToolkit.Html.Tokens
{
    abstract class TagToken : Token
    {
        public StringBuilder TagName { get; } = new StringBuilder();

        public List<AttributeToken> Attributes { get; } = new List<AttributeToken>();

        public bool IsSelfClosing { get; set; }
    }
}
