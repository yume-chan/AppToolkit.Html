using System;

namespace AppToolkit.Html.Interfaces
{
    internal enum DocumentHtmlType
    {
        Xml,
        Html
    }

    internal enum DocumentMode
    {
        NoQuirks,
        Quirks,
        LimitedQuirks
    }

    internal class DocumentState
    {
        public string Encoding { get; set; } = "utf-8";

        public string ContentType { get; set; } = "application/xml";

        public string Url { get; set; } = "about:blank";

        public string Orign { get; set; }

        public DocumentHtmlType Type { get; set; } = DocumentHtmlType.Xml;

        public DocumentMode Mode { get; set; } = DocumentMode.NoQuirks;

        public DocumentState Clone() => (DocumentState)MemberwiseClone();
    }

    internal interface IDocumentState
    {
        DocumentState State { get; }
    }

    internal interface IDocument : ParentNode
    {
        Node CloneNode(bool deep = false);

        DomImplementation Implementation { get; }
        string Url { get; }
        string DocumentUri { get; }
        string Origin { get; }
        string CompatMode { get; }
        string CharacterSet { get; }
        string ContentType { get; }

        DocumentType DocType { get; }
        Element DocumentElement { get; }
        HtmlCollection GetElementsByTagName(string localName);
        HtmlCollection GetElementsByTagNameNS(string @namespace, string localName);
        HtmlCollection GetElementsByClassName(string className);

        Element CreateElement(string localName);
        Element CreateElementNS(string @namespace, string qualifiedName);
        DocumentFragment CreateDocumentFragment();
        Text CreateTextNode(string data);
        Comment CreateComment(string data);
        ProcessingInstruction CreateProcessingInstruction(string target, string data);

        Node ImportNode(Node node, bool deep = false);
        Node AdoptNode(Node node);

        Event CreateEvent(string @interface);
        Range CreateRange();

        NodeIterator CreateNodeIterator(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null);
        TreeWalker CreateTreeWalker(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null);
    }

    internal interface IInnerDocument : IDocument
    {
        Document Wrapper { get; set; }
    }
}
