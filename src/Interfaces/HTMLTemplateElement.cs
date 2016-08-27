namespace AppToolkit.Html.Interfaces
{
    public class HtmlTemplateElement : HtmlElement
    {
        public const string Name = "template";

        private static readonly PropertyIdentity<bool> IsTemplateDocumentProperty = new PropertyIdentity<bool>("IsTemplateDocument", typeof(HtmlTemplateElement), false);
        private bool GetIsTemplateDocument(Document document) => document.GetValue(IsTemplateDocumentProperty);
        private void SetIsTemplateDocument(Document document, bool value) => document.SetValue(IsTemplateDocumentProperty, value);

        private static readonly PropertyIdentity<Document> TemplateDocumentProperty = new PropertyIdentity<Document>("TemplateDocument", typeof(HtmlTemplateElement), null);
        private Document GetTemplateDocument(Document document) => document.GetValue(TemplateDocumentProperty);
        private void SetTemplateDocument(Document document, Document value) => document.SetValue(TemplateDocumentProperty, value);

        internal HtmlTemplateElement(Document nodeDocument)
            : base(Name, nodeDocument)
        {
            if (!GetIsTemplateDocument(nodeDocument))
            {
                var templateDocument = (Document)nodeDocument.CloneNode();
                SetIsTemplateDocument(templateDocument, true);
                SetTemplateDocument(nodeDocument, templateDocument);
                nodeDocument = templateDocument;
            }
            Content = new DocumentFragment(nodeDocument);
        }

        public DocumentFragment Content { get; }
    }
}
