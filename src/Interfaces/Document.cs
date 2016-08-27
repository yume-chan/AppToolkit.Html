using System;
using System.Collections.Generic;

namespace AppToolkit.Html.Interfaces
{
    internal interface PropertyIdentity
    {
        string Name { get; }

        Type OwnerType { get; }
    }

    internal class PropertyIdentity<T> : PropertyIdentity, IEquatable<PropertyIdentity<T>>
    {
        public PropertyIdentity(string name, Type ownerType, T defaultValue)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));

            Name = name;
            OwnerType = ownerType;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public Type OwnerType { get; }

        public T DefaultValue { get; }

        public bool Equals(PropertyIdentity<T> other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (Name != other.Name)
                return false;
            if (OwnerType != other.OwnerType)
                return false;

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as PropertyIdentity<T>);

        public override int GetHashCode() => Name.GetHashCode() ^ OwnerType.GetHashCode();
    }

    public class Document : Node, ParentNode, IDocument, IDocumentState
    {
        private readonly Dictionary<PropertyIdentity, object> PropertyStore = new Dictionary<PropertyIdentity, object>();

        internal T GetValue<T>(PropertyIdentity<T> property) => PropertyStore.TryGetValue(property, out var value) ? (T)value : property.DefaultValue;

        internal void SetValue<T>(PropertyIdentity<T> property, T value) => PropertyStore[property] = value;

        internal readonly IInnerDocument InnerDocument;

        internal bool IsHtmlDocument => ((IDocumentState)this).State.Type == DocumentHtmlType.Html;

        public static explicit operator HtmlDocument(Document source)
        {
            if (source == null)
                return null;

            if (source.InnerDocument is HtmlDocument html)
                return html;

            throw new InvalidCastException();
        }

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

            InnerDocument = new HtmlDocument(state, this);

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

        internal override Node CloneOverride() => new Document(((IDocumentState)this).State.Clone());

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
        public DomImplementation Implementation => InnerDocument.Implementation;
        /// <summary>
        /// Returns <see cref="Document"/>'s URL.
        /// </summary>
        public string Url => InnerDocument.Url;
        /// <summary>
        /// Returns <see cref="Document"/>'s URL.
        /// </summary>
        public string DocumentUri => InnerDocument.DocumentUri;
        public string Origin => InnerDocument.Origin;
        public string CompatMode => InnerDocument.CompatMode;
        public string CharacterSet => InnerDocument.CharacterSet;
        public string ContentType => InnerDocument.ContentType;

        public DocumentType DocType => InnerDocument.DocType;
        public Element DocumentElement => InnerDocument.DocumentElement;

        public HtmlCollection GetElementsByTagName(string localName) => InnerDocument.GetElementsByTagName(localName);
        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName) => InnerDocument.GetElementsByTagNameNS(@namespace, localName);
        public HtmlCollection GetElementsByClassName(string className) => InnerDocument.GetElementsByClassName(className);

        public Element CreateElement(string localName) => InnerDocument.CreateElement(localName);
        public Element CreateElementNS(string @namespace, string qualifiedName) => InnerDocument.CreateElementNS(@namespace, qualifiedName);
        public DocumentFragment CreateDocumentFragment() => InnerDocument.CreateDocumentFragment();
        public Text CreateTextNode(string data) => InnerDocument.CreateTextNode(data);
        public Comment CreateComment(string data) => InnerDocument.CreateComment(data);
        public ProcessingInstruction CreateProcessingInstruction(string target, string data) => InnerDocument.CreateProcessingInstruction(target, data);

        public Node ImportNode(Node node, bool deep = false) => InnerDocument.ImportNode(node, deep);
        public Node AdoptNode(Node node) => InnerDocument.AdoptNode(node);

        public Event CreateEvent(string @interface) => InnerDocument.CreateEvent(@interface);

        public Range CreateRange() => InnerDocument.CreateRange();

        public NodeIterator CreateNodeIterator(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null) => InnerDocument.CreateNodeIterator(root, whatToShow, filter);
        public TreeWalker CreateTreeWalker(Node root, WhatToShow whatToShow = WhatToShow.All, NodeFilter filter = null) => InnerDocument.CreateTreeWalker(root, whatToShow, filter);

        #endregion

        #region Implement IDocumentState

        DocumentState IDocumentState.State => ((IDocumentState)InnerDocument).State;

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
