using System;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public class Document : Node<Document>, ParentNode
    {
        internal bool IsHtmlDocument { get; set; }

        public Document()
        {
            ownerDocument = this;
            Children = new ChildrenHtmlCollection(this);
        }

        public override NodeType NodeType => NodeType.Document;
        public override string NodeName => "#document";

        public DomImplementation Implementation { get; }
        public string Url { get; internal set; }
        public string DocumentUri { get; }
        public string Origin { get; }
        public string CompatMode { get; }
        public string CharacterSet { get; }
        public string ContentType { get; internal set; }

        public DocumentType DocType { get; }
        public Element DocumentElement { get; internal set; }

        public HtmlCollection Children { get; }

        public Element FirstElementChild => ChildNodes.OfType<Element>().FirstOrDefault();

        public Element LastElementChild => ChildNodes.OfType<Element>().LastOrDefault();

        public uint ChildElementCount => Children.Length;

        public HtmlCollection GetElementsByTagName(string localName) { throw new NotImplementedException(); }
        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName) { throw new NotImplementedException(); }
        public HtmlCollection GetElementsByClassName(string className) { throw new NotImplementedException(); }

        public Element CreateElement(string localName)
        {
            switch (localName)
            {
                case "head":
                    return new HtmlHeadElement() { ownerDocument = this };
                default:
                    return new HtmlUnknownElement() { LocalName = localName, ownerDocument = this };
            }
        }
        public Element CreateElementNS(string @namespace, string localName) { throw new NotImplementedException(); }
        public DocumentFragment CreateDocumentFragmen() { throw new NotImplementedException(); }
        public Text CreateTextNode(string data) => new Text(data) { ownerDocument = this };
        public Comment CreateComment(string data) => new Comment(data) { ownerDocument = this };
        public ProcessingInstruction CreateProcessingInstruction(string target, string data) { throw new NotImplementedException(); }

        public Node ImportNode(Node node, bool deep = false) { throw new NotImplementedException(); }
        public Node AdoptNode(Node node)
        {
            if (node is Document)
                throw new DomException("NotSupportedError");

            node.ParentNode?.RemoveChild(node);
            node.ownerDocument = this;

            return node;
        }

        public Event CreateEvent(string @interface) { throw new NotImplementedException(); }

        public Range CreateRange() { throw new NotImplementedException(); }

        public NodeIterator CreateNodeIterator(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null) => new NodeIterator(root, whatToShow, filter);
        public TreeWalker CreateTreeWalker(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null) => new TreeWalker(root, whatToShow, filter);

        internal override Node CloneOverride() => new Document() { Url = Url, ContentType = ContentType };

        internal override string LookupPrefixOverride(string @namespace) => DocumentElement?.LookupPrefixOverride(@namespace);
        internal override string LookupNamespaceUriOverride(string prefix) => DocumentElement?.LookupNamespaceUriOverride(prefix);

        public Element QuerySelector(string selectors)
        {
            throw new NotImplementedException();
        }

        public NodeList QuerySelectorAll(string selectors)
        {
            throw new NotImplementedException();
        }
    }

    public class XmlDocument : Document
    {
        public XmlDocument()
        {
            IsHtmlDocument = false;
        }
    }

    public class HtmlDocument : Document
    {
        public HtmlDocument()
        {
            IsHtmlDocument = true;
        }

        public string Domain { get; set; }
        public string Referer { get; }
        public string Cookie { get; set; }
        public string LastModified { get; }

        public string Title { get; set; }
        public string Dir { get; set; }
        public HtmlElement Body { get; set; }
        public HtmlHeadElement Head => DocumentElement?.ChildNodes.OfType<HtmlHeadElement>().FirstOrDefault();
        public HtmlCollection Images { get; }
        public HtmlCollection Embeds { get; }
        public HtmlCollection Plugins { get; }
        public HtmlCollection Links { get; }
        public HtmlCollection Forms { get; }
        public HtmlCollection Scripts { get; }
        public NodeList GetElementsByName(string elementName) { throw new NotImplementedException(); }
    }

}
