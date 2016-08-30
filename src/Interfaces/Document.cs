using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public Uri Url { get; set; } = new Uri("about:blank");

        public string Orign { get; set; }

        public DocumentHtmlType Type { get; set; } = DocumentHtmlType.Xml;

        public DocumentMode Mode { get; set; } = DocumentMode.NoQuirks;

        public DocumentState Clone() => (DocumentState)MemberwiseClone();
    }

    public class Document : Node, ParentNode
    {
        private readonly Dictionary<PropertyIdentity, object> PropertyStore = new Dictionary<PropertyIdentity, object>();

        internal T GetValue<T>(PropertyIdentity<T> property) => PropertyStore.TryGetValue(property, out var value) ? (T)value : property.DefaultValue;

        internal void SetValue<T>(PropertyIdentity<T> property, T value) => PropertyStore[property] = value;

        internal bool IsHtmlDocument => State.Type == DocumentHtmlType.Html;

        internal DocumentState State { get; }

        /// <summary>
        /// Returns a new <see cref="Document"/>.
        /// </summary>
        public Document()
            : this(new DocumentState())
        { }

        internal Document(DocumentState state)
            : base(null)
        {
            OwnerDocument = this;
            State = state;

            ParentNodeImplementation = new ParentNodeImplementation(this);
        }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.Document;
        public override string NodeName => "#document";

        internal override string LookupPrefixOverride(string @namespace) => DocumentElement?.LookupPrefixOverride(@namespace);
        internal override string LookupNamespaceUriOverride(string prefix) => DocumentElement?.LookupNamespaceUriOverride(prefix);

        internal override Node CloneOverride() => new Document(State.Clone());

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

        #region Standard Document
        // https://dom.spec.whatwg.org/#interface-document

        /// <summary>
        /// Returns <see cref="Document"/>'s <see cref="DOMImplementation"/> object. 
        /// </summary>
        public DomImplementation Implementation { get { throw new NotImplementedException(); } }
        /// <summary>
        /// Returns <see cref="Document"/>'s URL.
        /// </summary>
        public string Url => State.Url.ToString();
        /// <summary>
        /// Returns <see cref="Document"/>'s URL.
        /// </summary>
        public string DocumentUri => State.Url.ToString();
        public string Origin => State.Orign;
        public string CompatMode => State.Mode == DocumentMode.Quirks ? "BackCompat" : "CSS1Compat";
        public string CharacterSet => State.Encoding;
        public string ContentType => State.ContentType;

        public DocumentType DocType { get { throw new NotImplementedException(); } }
        public Element DocumentElement { get; internal set; }

        public HtmlCollection GetElementsByTagName(string qualifiedName)
        {
            if (qualifiedName == "*")
                return new HtmlCollection(new List<Element>(this.GetDescendants()));

            var lower = qualifiedName.ToLower();

            var list = new List<Element>();
            foreach (var item in this.GetDescendants())
            {
                if (item.NamespaceUri == HtmlElement.HtmlNamespace)
                {
                    if (item.QualifiedName == lower)
                        list.Add(item);
                }
                else if (item.QualifiedName == qualifiedName)
                {
                    list.Add(item);
                }
            }
            return new HtmlCollection(list);
        }
        public HtmlCollection<T> GetElementsByTagName<T>(string qualifiedName) where T : Element => GetElementsByTagName(qualifiedName).Cast<T>();
        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName) { throw new NotImplementedException(); }
        private void GetElementsByClassName(HashSet<string> className, List<Element> result)
        {
            foreach (var item in Children)
            {
                if (className.IsSupersetOf(item.ClassList))
                    result.Add(item);
                GetElementsByClassName(className, result);
            }
        }
        public HtmlCollection GetElementsByClassName(string className)
        {
            var set = new HashSet<string>(className.Split(' '));
            if (set.Count == 0)
                return HtmlCollection.Empty;

            var list = new List<Element>();
            foreach (var item in this.GetDescendants())
                if (set.IsSubsetOf(item.ClassList))
                    list.Add(item);
            return new HtmlCollection(list);
        }

        private const string NameStartChar = "A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\x2FF\x370-\x37D\x37F-\x1FFF\x200C-\x200D\x2070-\x218F\x2C00-\x2FEF\x3001-\xD7FF\xF900-\xFDCF\xFDF0-\xFFFD\x10000-\xEFFFF";
        private const string NameChar = NameStartChar + "\\-.0-9\xB7\x0300-\x036F\x203F-\x2040";
        private static Regex Name = new Regex("[:" + NameStartChar + "][" + NameChar + "]*");
        private static Regex NcName = new Regex("[" + NameStartChar + "][" + NameChar + "]*");
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

        private Element CreateElement(string localName, string prefix)
        {
            localName = localName.ToLower();

            switch (localName)
            {
                case HtmlAnchorElement.Name:
                    return new HtmlAnchorElement(this, prefix);
                case HtmlHtmlElement.Name:
                    return new HtmlHtmlElement(this, prefix);
                case HtmlImageElement.Name:
                    return new HtmlImageElement(this, prefix);
                case HtmlHeadElement.Name:
                    return new HtmlHeadElement(this, prefix);
                case HtmlTemplateElement.Name:
                    return new HtmlTemplateElement(this, prefix);
                default:
                    return new HtmlUnknownElement(localName, this, prefix);
            }
        }

        public Element CreateElement(string localName)
        {
            if (!Name.IsMatch(localName))
                throw new DomException(DomExceptionCode.InvalidCharacterError);

            return CreateElement(localName, null);
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

            return CreateElement(localName, prefix);
        }
        public DocumentFragment CreateDocumentFragment() { throw new NotImplementedException(); }
        public Text CreateTextNode(string data) => new Text(data, this);
        public Comment CreateComment(string data) => new Comment(data, this);
        public ProcessingInstruction CreateProcessingInstruction(string target, string data) => new ProcessingInstruction(target, data, this);

        public Node ImportNode(Node node, bool deep = false) { throw new NotImplementedException(); }
        public Node AdoptNode(Node node)
        {
            if (node.NodeType == NodeType.Document)
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
    }

    public class XmlDocument : Document
    {
        public XmlDocument()
        {
        }

        internal override Node CloneOverride() => new XmlDocument();
    }
}
