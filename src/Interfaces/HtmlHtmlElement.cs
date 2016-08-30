namespace AppToolkit.Html.Interfaces
{
    public class HtmlHtmlElement : HtmlElement
    {
        internal const string Name = "html";

        public HtmlHtmlElement(Document nodeDocument, string prefix = null)
            : base(Name, nodeDocument, prefix)
        { }
    }
}
