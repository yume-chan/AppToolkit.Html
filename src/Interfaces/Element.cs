using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AppToolkit.Html.Interfaces
{
    public class Element : Node, ParentNode
    {
        internal const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        internal Element(string localName, Document nodeDocument)
            : base(nodeDocument)
        {
            LocalName = localName;
            ParentNodeImplementation = new ParentNodeImplementation(this);

            SetAttribute("class", "");
            ClassList = new DomTokenList(GetAttributeNode("class"));
        }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.Element;
        public override string NodeName => TagName;

        internal override Node CloneOverride()
        {
            var element = new Element(LocalName, OwnerDocument)
            {
                NamespaceUri = NamespaceUri,
                Prefix = Prefix
            };
            foreach (var attr in AttributeList)
                element.AppendAttribute((Attr)attr.CloneNode());
            return element;
        }
        protected override bool IsEqualNodeOverride(Node node)
        {
            var element = (Element)node;

            if (NamespaceUri != element.NamespaceUri)
                return false;

            if (Prefix != element.Prefix)
                return false;

            if (LocalName != element.LocalName)
                return false;

            if (!AttributeList.SequenceEqual(element.AttributeList))
                return false;

            return true;
        }

        internal override string LookupPrefixOverride(string @namespace)
        {
            if (@namespace == NamespaceUri && Prefix != null)
                return Prefix;

            var attr = AttributeList.FirstOrDefault(x => x.Prefix == "xmlns" && x.Value == @namespace);
            if (attr != null)
                return attr.LocalName;

            return base.LookupPrefixOverride(@namespace);
        }
        internal override string LookupNamespaceUriOverride(string prefix)
        {
            if (Prefix == prefix && NamespaceUri != null)
                return NamespaceUri;

            Attr attr;
            if (prefix != null)
                attr = AttributeList.FirstOrDefault(x => x.NamespaceUri == XmlnsNamespace && x.Prefix == "xmlns" && x.LocalName == prefix);
            else
                attr = AttributeList.FirstOrDefault(x => x.NamespaceUri == XmlnsNamespace && x.Prefix == null && x.LocalName == "xmlns");

            if (attr?.Value is string value && value != string.Empty)
                return value;

            return base.LookupNamespaceUriOverride(prefix);
        }

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

        #region Standard DOM Standard
        // https://dom.spec.whatwg.org/#element

        public string NamespaceUri { get; internal set; }
        public string Prefix { get; internal set; }
        public string LocalName { get; }
        public string TagName
        {
            get
            {
                string qualifiedName;
                if (Prefix == null)
                    qualifiedName = LocalName;
                else
                    qualifiedName = $"{Prefix}:{LocalName}";

                if (NamespaceUri == HtmlElement.HtmlNamespace &&
                    OwnerDocument.IsHtmlDocument)
                    return qualifiedName.ToUpper();
                else
                    return qualifiedName;
            }
        }

        public string Id
        {
            get { return GetAttribute("id") ?? string.Empty; }
            set { SetAttribute("id", value); }
        }
        public string ClassName
        {
            get { return GetAttribute("class") ?? string.Empty; }
            set { SetAttribute("class", value); }
        }
        public DomTokenList ClassList { get; private set; }
        public string Slot { get; set; }

        private readonly List<Attr> AttributeList = new List<Attr>();

        /// <summary>
        /// Returns <c>false</c> if context object’s attribute list is empty, and <c>true</c> otherwise.
        /// </summary>
        /// <returns>
        /// Returns <c>false</c> if context object’s attribute list is empty, and <c>true</c> otherwise.
        /// </returns>
        public bool HasAttributes() => AttributeList.Count != 0;
        public NamedDomNodeMap Attributes { get; }
        public IEnumerable<string> GetAttributeNames() { throw new NotImplementedException(); }
        public string GetAttribute(string qualifiedName) => GetAttributeNode(qualifiedName)?.Value;
        public string GetAttributeNS(string @namespace, string localName) => GetAttributeNodeNS(@namespace, localName)?.Value;

        internal const string XmlNameStartCharRegex = "[:A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\x2FF\x370-\x37D\x37F-\x1FFF\x200C-\x200D\x2070-\x218F\x2C00-\x2FEF\x3001-\xD7FF\xF900-\xFDCF\xFDF0-\xFFFD\x10000-\xEFFFF";
        internal static readonly Regex XmlNameRegex = new Regex("^" + XmlNameStartCharRegex + "]" + XmlNameStartCharRegex + "\\-.0-9\xB7\x300-\x36F\x203F-\x2040]*$");

        internal void AppendAttribute(Attr attr)
        {
            AttributeList.Add(attr);
        }
        internal void ChangeAttribute(Attr attr, string value)
        {
            attr.IsReal = true;
            attr.Value = value;
        }
        internal void RemoveAttribute(Attr attr)
        {
            if (attr.Name == "class")
                attr.IsReal = false;
            else
                AttributeList.Remove(attr);
        }

        public void SetAttribute(string qualifiedName, string value)
        {
            if (!XmlNameRegex.IsMatch(qualifiedName))
                throw new DomException(DomExceptionCode.InvalidCharacterError);

            if (OwnerDocument.IsHtmlDocument)
                qualifiedName = qualifiedName.ToLower();

            var attr = GetAttributeNode(qualifiedName);
            if (attr == null)
            {
                attr = new Attr(qualifiedName, value, this, OwnerDocument);
                AppendAttribute(attr);
            }
            else
            {
                ChangeAttribute(attr, value);
            }
        }
        public void SetAttributeNS(string @namespace, string name, string value) { throw new NotImplementedException(); }
        public void RemoveAttribute(string name)
        {
            if (OwnerDocument.IsHtmlDocument)
                name = name.ToLower();

            RemoveAttribute(AttributeList.FirstOrDefault(x => x.Name == name));
        }
        public void RemoveAttributeNS(string @namespace, string name) { throw new NotImplementedException(); }
        public bool HasAttribute(string name)
        {
            if (OwnerDocument.IsHtmlDocument)
                name = name.ToLower();

            return AttributeList.FirstOrDefault(x => x.Name == name) != null;
        }
        public bool HasAttributeNS(string @namespace, string localName) { throw new NotImplementedException(); }

        public Attr GetAttributeNode(string qualifiedName)
        {
            if (OwnerDocument.IsHtmlDocument)
                qualifiedName = qualifiedName.ToLower();

            foreach (var item in AttributeList)
                if (item.IsReal && item.NodeName == qualifiedName)
                    return item;

            return null;
        }
        public Attr GetAttributeNodeNS(string @namespace, string localName)
        {
            if (@namespace == string.Empty)
                @namespace = null;

            foreach (var item in AttributeList)
                if (item.IsReal && item.NamespaceUri == @namespace && item.LocalName == localName)
                    return item;

            return null;
        }
        public Attr SetAttributeNode(Attr attr)
        {
            if (attr.OwnerElement != null && attr.OwnerElement != this)
                throw new DomException(DomExceptionCode.InUseAttributeError);

            var old = GetAttributeNodeNS(attr.NamespaceUri, attr.LocalName);
            if (old != null)
                AttributeList.Remove(old);

            AttributeList.Add(attr);

            if (attr.LocalName == "class")
                ClassList = new DomTokenList(attr);

            return old;
        }
        public Attr SetAttributeNodeNS(Attr attr) { throw new NotImplementedException(); }
        public Attr RemoveAttributeNode(Attr attr) { throw new NotImplementedException(); }

        public HtmlCollection GetElementsByTagName(string localName) { throw new NotImplementedException(); }
        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName) { throw new NotImplementedException(); }
        public HtmlCollection GetElementsByClassName(string className) { throw new NotImplementedException(); }

        public override string TextContent
        {
            get
            {
                return string.Concat(ChildNodes.Select(x => x.TextContent));
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ReplaceAll(null);
                else
                    ReplaceAll(new Text(value));
            }
        }

        #endregion

        #region Extension DOM Parsing and Serialization
        // https://w3c.github.io/DOM-Parsing/#extensions-to-the-element-interface

        private static string EscapeString(string input, bool attributeMode)
        {
            input = input.Replace("&", "&amp;").Replace("\u00A0", "&nbsp;");
            if (attributeMode)
                return input.Replace("\"", "&quot;");
            else
                return input.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        class OneOfMatcher
        {
            public ImmutableArray<string> Items { get; }

            public OneOfMatcher(params string[] items)
            {
                Items = items.ToImmutableArray();
            }

            public static bool operator ==(string input, OneOfMatcher matcher) => matcher.Items.Contains(input);

            public static bool operator !=(string input, OneOfMatcher matcher) => !(input == matcher);

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return base.ToString();
            }
        }

        static OneOfMatcher OneOf(params string[] items) => new OneOfMatcher(items);

        private static void SerializeHtmlFragmentCore(Node node, StringBuilder builder)
        {
            switch (node)
            {
                case Element element:
                    var tagName = element.TagName;

                    builder.EnsureCapacity(builder.Length + 2 * tagName.Length + 5);
                    builder.Append('<').Append(tagName);
                    foreach (var attr in element.AttributeList)
                        builder.Append($" {attr.Name}=\"{EscapeString(attr.Value, true)}\"");
                    builder.Append('>');
                    SerializeHtmlFragment(element, builder);
                    builder.Append($"</{tagName}>");
                    break;
                case Text text:
                    if (text.ParentNode is Element parent &&
                        parent.TagName == OneOf("style", "script", "xmp", "iframe", "noembed", "noframes"))
                        builder.Append(text.Data);
                    else
                        builder.Append(EscapeString(text.Data, false));
                    break;
                case Comment comment:
                    builder.EnsureCapacity(builder.Capacity + comment.Data.Length + 7);
                    builder.Append("<!--").Append(comment.Data).Append("-->");
                    break;
                case ProcessingInstruction instruction:
                    builder.EnsureCapacity(builder.Capacity + instruction.Target.Length + instruction.Data.Length + 4);
                    builder.Append("<?").Append(instruction.Target).Append(" ").Append(instruction.Data).Append(">");
                    break;
                case DocumentType type:
                    builder.EnsureCapacity(builder.Capacity + type.Name.Length + 11);
                    builder.Append("<!DOCTYPE ").Append(type.Name).Append(">");
                    break;
            }
        }

        private static void SerializeHtmlFragment(Node node, StringBuilder builder)
        {
            if (node is HtmlTemplateElement template)
                node = template.Content;

            foreach (var child in node.ChildNodes)
                SerializeHtmlFragmentCore(child, builder);
        }

        /// <summary>
        /// Returns a fragment of HTML or XML that represents the element's contents.
        /// Can be set, to replace the contents of the element with nodes parsed from the given string. 
        /// </summary>
        /// <exception cref="DomException">
        /// In the case of an XML document,
        /// throws a <see cref="DomExceptionCode.InvalidStateError"/> DOMException if the <see cref="Element"/> cannot be serialized to XML,
        /// or a <see cref="DomExceptionCode.SyntaxError"/> DOMException if the given string is not well-formed. 
        /// </exception>
        public string InnerHtml
        {
            get
            {
                var builder = new StringBuilder();
                SerializeHtmlFragment(this, builder);
                return builder.ToString();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a fragment of HTML or XML that represents the element and its contents. 
        /// Can be set, to replace the element with nodes parsed from the given string. 
        /// </summary>
        /// <exception cref="DomException">
        /// In the case of an XML document,
        /// throws a <see cref="DomExceptionCode.InvalidStateError"/> DOMException if the <see cref="Element"/> cannot be serialized to XML,
        /// or a <see cref="DomExceptionCode.SyntaxError"/> DOMException if the given string is not well-formed. 
        /// 
        /// Throws a <see cref="DomExceptionCode.NoModificationAllowedError"/> DOMException if the parent of the element is a <see cref="Document"/>. 
        /// </exception>
        public string OuterHtml
        {
            get
            {
                var builder = new StringBuilder();
                SerializeHtmlFragmentCore(this, builder);
                return builder.ToString();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Parses the given string <paramref name="text"/> as HTML or XML and inserts the resulting nodes
        /// into the tree in the position given by the <paramref name="position"/> argument.
        /// </summary>
        public void InsertAdjacentHTML(string position, string text) { throw new NotImplementedException(); }

        #endregion
    }
}
