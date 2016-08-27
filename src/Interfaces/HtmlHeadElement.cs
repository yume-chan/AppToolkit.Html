namespace AppToolkit.Html.Interfaces
{
    public class HtmlHeadElement : HtmlElement
    {
        public const string Name = "head";

        internal HtmlHeadElement(Document nodeDocument)
            : base(Name, nodeDocument)
        { }
    }
}
