using System.Text;

namespace AppToolkit.Html.Tokens
{
    public class AttributeToken
    {
        public StringBuilder Name { get; }

        public StringBuilder Value { get; }

        public AttributeToken()
        {
            Name = new StringBuilder();
            Value = new StringBuilder();
        }
    }
}
