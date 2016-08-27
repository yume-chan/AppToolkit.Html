using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace AppToolkit.Html.Tokens
{
    internal abstract class TagToken : Token
    {
        public string TagName { get; set; }

        public ImmutableArray<AttributeToken> Attributes { get; }

        public bool IsSelfClosing { get; }

        public TagToken(string tagName, ImmutableArray<AttributeToken> attributes, bool isSelfClosing)
        {
            TagName = tagName;
            Attributes = attributes;
            IsSelfClosing = isSelfClosing;
        }
    }

    internal class TagTokenBuilder
    {
        public bool IsStartTag { get; private set; }

        public string LastStartTagName { get; private set; }

        public StringBuilder TagName { get; } = new StringBuilder();

        public List<AttributeToken> Attributes { get; } = new List<AttributeToken>();

        public bool IsSelfClosing { get; set; }

        public TagToken Create()
        {
            TagToken result;
            if (IsStartTag)
            {
                result = new StartTagToken(TagName.ToString(), Attributes.ToImmutableArray(), IsSelfClosing);
                LastStartTagName = result.TagName;
            }
            else
            {
                result = new EndTagToken(TagName.ToString(), Attributes.ToImmutableArray(), IsSelfClosing);
            }

            TagName.Clear();
            Attributes.Clear();
            IsSelfClosing = false;

            return result;
        }

        public void New(bool isStartTag)
        {
            IsStartTag = isStartTag;

            TagName.Clear();
            Attributes.Clear();
            IsSelfClosing = false;
        }

        public bool IsAppropriateEndTagToken() => TagName.ToString() == LastStartTagName;
    }
}
