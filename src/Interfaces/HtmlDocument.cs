using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppToolkit.Html.Interfaces
{
    public class HtmlDocument : Node, ParentNode, IInnerDocument, IDocumentState
    {
        Document IInnerDocument.Wrapper { get; set; }

        public static implicit operator Document(HtmlDocument source) => ((IInnerDocument)source)?.Wrapper;

        internal HtmlDocument(Document document)
            : this(new DocumentState(), document)
        { }

        internal HtmlDocument(DocumentState state, Document document)
            : base(document)
        {
            state.Type = DocumentHtmlType.Html;
            State = state;

            ((IInnerDocument)this).Wrapper = document;

            ParentNodeImplementation = new ParentNodeImplementation(this);
        }

        #region Override Node

        public override NodeType NodeType => NodeType.Document;
        public override string NodeName => "#document";

        internal override string LookupPrefixOverride(string @namespace) => DocumentElement?.LookupPrefixOverride(@namespace);
        internal override string LookupNamespaceUriOverride(string prefix) => DocumentElement?.LookupNamespaceUriOverride(prefix);

        internal override Node CloneOverride() => (HtmlDocument)(Document)((IInnerDocument)this).Wrapper.CloneNode();

        #endregion

        #region Implement ParentNode

        private readonly ParentNodeImplementation ParentNodeImplementation;

        /// <summary>
        /// Returns the child <see cref="Element"/>s.
        /// </summary>
        public HtmlCollection Children => ParentNodeImplementation.Children;
        /// <summary>
        /// Returns the first child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        public Element FirstElementChild => ParentNodeImplementation.FirstElementChild;
        /// <summary>
        /// Returns the last child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        public Element LastElementChild => ParentNodeImplementation.LastElementChild;
        /// <summary>
        /// Returns the number of children of context object that are <see cref="Element"/>. 
        /// </summary>
        public uint ChildElementCount => ParentNodeImplementation.ChildElementCount;

        /// <summary>
        /// Inserts <paramref name="nodes"/> before the first child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        public void Prepend(params object[] nodes) => ParentNodeImplementation.Prepend(nodes);
        /// <summary>
        /// Inserts <paramref name="nodes"/> after the last child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        public void Append(params object[] nodes) => ParentNodeImplementation.Append(nodes);

        /// <summary>
        /// Returns the first element that is a descendant of node that matches <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the first result of running scope-match a selectors string <paramref name="selectors"/> against context object,
        /// if the result is not an empty list, and <code>null</code> otherwise. 
        /// </returns>
        public Element QuerySelector(string selectors) => ParentNodeImplementation.QuerySelector(selectors);
        /// <summary>
        /// Returns all element descendants of node that match <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the static result of running scope-match a selectors string <paramref name="selectors"/> against context object.
        /// </returns>
        public NodeList QuerySelectorAll(string selectors) => ParentNodeImplementation.QuerySelectorAll(selectors);

        #endregion

        #region Implement IDocument

        /// <summary>
        /// Returns <see cref="Document"/>'s <see cref="DOMImplementation"/> object. 
        /// </summary>
        public DomImplementation Implementation { get { throw new NotImplementedException(); } }
        /// <summary>
        /// Returns <see cref="Document"/>'s URL.
        /// </summary>
        public string Url => State.Url;
        /// <summary>
        /// Returns <see cref="Document"/>'s URL.
        /// </summary>
        public string DocumentUri => State.Url;
        public string Origin => State.Orign;
        public string CompatMode => State.Mode == DocumentMode.Quirks ? "BackCompat" : "CSS1Compat";
        public string CharacterSet => State.Encoding;
        public string ContentType => State.ContentType;

        public DocumentType DocType { get { throw new NotImplementedException(); } }
        public Element DocumentElement { get; internal set; }

        public HtmlCollection GetElementsByTagName(string localName) { throw new NotImplementedException(); }
        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName) { throw new NotImplementedException(); }
        public HtmlCollection GetElementsByClassName(string className) { throw new NotImplementedException(); }

        private const string NameStartChar = "A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\x2FF\x370-\x37D\x37F-\x1FFF\x200C-\x200D\x2070-\x218F\x2C00-\x2FEF\x3001-\xD7FF\xF900-\xFDCF\xFDF0-\xFFFD\x10000-\xEFFFF";
        private const string NameChar = NameStartChar + "\\-.0-9\xB7\x0300-\x036F\x203F-\x2040";
        private static Regex Name = new Regex("[:" + NameStartChar + "][" + NameChar + "]*");
        private static Regex NcName = new Regex("[" + NameStartChar + "][" + NameChar + "]*");
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        public Element CreateElement(string localName)
        {
            if (!Name.IsMatch(localName))
                throw new DomException(DomExceptionCode.InvalidCharacterError);

            localName = localName.ToLower();

            switch (localName)
            {
                case HtmlHeadElement.Name:
                    return new HtmlHeadElement(this);
                case HtmlTemplateElement.Name:
                    return new HtmlTemplateElement(this);
                default:
                    return new HtmlUnknownElement(localName, this);
            }
        }
        public Element CreateElementNS(string @namespace, string qualifiedName)
        {
            if (!Name.IsMatch(qualifiedName))
                throw new DomException(DomExceptionCode.InvalidCharacterError);

            var prefix = (string)null;
            var localName = (string)null;

            var parts = qualifiedName.Split(':');
            if (parts.Length > 2)
                throw new DomException(DomExceptionCode.NamespaceError);

            if (parts.Length == 2)
            {
                prefix = parts[0];
                localName = parts[1];
            }
            else
            {
                localName = parts[0];
            }

            if (prefix != null && @namespace == null)
                throw new DomException(DomExceptionCode.NamespaceError);

            if (prefix == "xml" && @namespace != XmlNamespace)
                throw new DomException(DomExceptionCode.NamespaceError);

            if (prefix == "xmlns" && @namespace != Element.XmlnsNamespace)
                throw new DomException(DomExceptionCode.NamespaceError);

            if (prefix == "xmlns" && @namespace == Element.XmlnsNamespace)
                throw new DomException(DomExceptionCode.NamespaceError);

            if (@namespace != HtmlElement.HtmlNamespace)
                throw new NotSupportedException();

            var result = CreateElement(localName);
            result.Prefix = prefix;
            return result;
        }
        public DocumentFragment CreateDocumentFragment() { throw new NotImplementedException(); }
        public Text CreateTextNode(string data) => new Text(data, this);
        public Comment CreateComment(string data) => new Comment(data, this);
        public ProcessingInstruction CreateProcessingInstruction(string target, string data) => new ProcessingInstruction(target, data, this);

        public Node ImportNode(Node node, bool deep = false) { throw new NotImplementedException(); }
        public Node AdoptNode(Node node)
        {
            if (node is IDocument)
                throw new DomException(DomExceptionCode.NotSupportedError);

            node.ParentNode?.RemoveChild(node);

            void AdoptDescendants(Node target, Document document)
            {
                target.OwnerDocument = document;
                if (!target.HasChildNodes())
                    return;

                foreach (var item in target.ChildNodes)
                    AdoptDescendants(item, document);
            }

            AdoptDescendants(node, this);

            return node;
        }

        public Event CreateEvent(string @interface) { throw new NotImplementedException(); }

        public Range CreateRange() { throw new NotImplementedException(); }

        public NodeIterator CreateNodeIterator(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null) => new NodeIterator(root, whatToShow, filter);
        public TreeWalker CreateTreeWalker(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null) => new TreeWalker(root, whatToShow, filter);

        #endregion

        #region Implement IDocumentState

        internal DocumentState State { get; }

        DocumentState IDocumentState.State => State;

        #endregion

        #region Extension WHATWG HTML Standard
        // https://html.spec.whatwg.org/multipage/dom.html#the-document-object

        public string Domain { get; }
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

        Document Open(string type = "text/html", string replace = "") { throw new NotImplementedException(); }
        void Close() { throw new NotImplementedException(); }
        void Write(params string[] text) { throw new NotImplementedException(); }
        void WriteLn(params string[] text) { throw new NotImplementedException(); }

        #endregion
    }
}
