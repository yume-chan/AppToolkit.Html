namespace AppToolkit.Html.Interfaces
{
    public class HtmlUnknownElement : HtmlElement
    {
        internal HtmlUnknownElement(string localName, Document nodeDocument, string prefix = null)
            : base(localName, nodeDocument, prefix)
        { }
    }
}
