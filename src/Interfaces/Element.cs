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
        internal Element()
        {
            Children = new ChildrenHtmlCollection(this);
        }

        public override NodeType NodeType => NodeType.Element;
        public override string NodeName => TagName;

        public string NamespaceUri { get; internal set; }
        public string Prefix { get; internal set; }
        public string LocalName { get; internal set; }
        public string TagName
        {
            get
            {
                if (NamespaceUri == null)
                    return LocalName;
                else
                    return $"{Prefix}:{LocalName}";
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
        public DomTokenList ClassList { get; }

        internal List<Attr> AttributeList { get; } = new List<Attr>();

        public NamedDomNodeMap Attributes { get; }
        public string GetAttribute(string name)
        {
            if (nodeDocument.IsHtmlDocument)
                name = name.ToLower();

            foreach (var attr in AttributeList)
                if (attr.Name == name)
                    return attr.Value;

            return null;
        }
        public string GetAttributeNS(string @namespace, string localName)
        {
            if (@namespace == string.Empty)
                @namespace = null;

            foreach (var attr in AttributeList)
                if (attr.NamespaceUri == @namespace &&
                    attr.Name == localName)
                    return attr.Value;

            return null;
        }

        internal const string XmlNameStartCharRegex = "[:A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\x2FF\x370-\x37D\x37F-\x1FFF\x200C-\x200D\x2070-\x218F\x2C00-\x2FEF\x3001-\xD7FF\xF900-\xFDCF\xFDF0-\xFFFD\x10000-\xEFFFF";
        internal static readonly Regex XmlNameRegex = new Regex("^" + XmlNameStartCharRegex + "]" + XmlNameStartCharRegex + "\\-.0-9\xB7\x300-\x36F\x203F-\x2040]*$");

        internal void AppendAttribute(Attr attr)
        {
            AttributeList.Add(attr);
        }
        internal void ChangeAttribute(Attr attr, string value)
        {
            attr.Value = value;
        }
        internal void RemoveAttribute(Attr attr)
        {
            AttributeList.Remove(attr);
        }

        public void SetAttribute(string name, string value)
        {
            if (!XmlNameRegex.IsMatch(name))
                throw new DomException("InvalidCharacterError");

            if (nodeDocument.IsHtmlDocument)
                name = name.ToLower();

            var attr = AttributeList.FirstOrDefault(x => x.Name == name);
            if (attr == null)
            {
                attr = new Attr(name, value);
                AppendAttribute(attr);
            }
            else
                ChangeAttribute(attr, value);
        }
        public void SetAttributeNS(string @namespace, string name, string value) { throw new NotImplementedException(); }
        public void RemoveAttribute(string name)
        {
            if (nodeDocument.IsHtmlDocument)
                name = name.ToLower();

            RemoveAttribute(AttributeList.FirstOrDefault(x => x.Name == name));
        }
        public void RemoveAttributeNS(string @namespace, string name) { throw new NotImplementedException(); }
        public bool HasAttribute(string name)
        {
            if (nodeDocument.IsHtmlDocument)
                name = name.ToLower();

            return AttributeList.FirstOrDefault(x => x.Name == name) != null;
        }
        public bool HasAttributeNS(string @namespace, string localName) { throw new NotImplementedException(); }

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

        public HtmlCollection Children { get; }

        public Element FirstElementChild => ChildNodes.OfType<Element>().FirstOrDefault();

        public Element LastElementChild => ChildNodes.OfType<Element>().LastOrDefault();

        public uint ChildElementCount => Children.Length;

        internal override Node CloneOverride()
        {
            var element = new Element()
            {
                NamespaceUri = NamespaceUri,
                Prefix = Prefix,
                LocalName = LocalName
            };
            foreach (var attr in AttributeList)
                element.AttributeList.Add(attr.Clone());
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

            return ParentElement?.LookupPrefixOverride(@namespace);
        }

        internal override string LookupNamespaceUriOverride(string prefix)
        {
            if (Prefix == prefix && NamespaceUri != null)
                return NamespaceUri;

            Attr attr;
            if (prefix != null)
                attr = AttributeList.FirstOrDefault(x => x.NamespaceUri == "http://www.w3.org/2000/xmlns/" && x.Prefix == "xmlns" && x.LocalName == prefix);
            else
                attr = AttributeList.FirstOrDefault(x => x.NamespaceUri == "http://www.w3.org/2000/xmlns/" && x.Prefix == null && x.LocalName == "xmlns");

            if (attr != null)
                return attr.Value;

            return ParentElement?.LookupNamespaceUriOverride(prefix);
        }

        public Element QuerySelector(string selectors)
        {
            throw new NotImplementedException();
        }

        public NodeList QuerySelectorAll(string selectors)
        {
            throw new NotImplementedException();
        }

        #region DOM5

        public string InnerHtml
        {
            get { return null; }
            set { }
        }

        private static string EscapeString(string input, bool attributeMode)
        {
            input = input.Replace("&", "&amp;");
            input = input.Replace("\u00A0", "&nbsp;");
            if (attributeMode)
                input = input.Replace("\"", "&quot;");
            else
                input = input.Replace("<", "&lt;").Replace(">", "&gt;");
            return input;
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

        private static string SerializeHtmlFragment(Node node)
        {
            var builder = new StringBuilder();

            if (node is HtmlTemplateElement template)
                node = template.Content;

            foreach (var child in node.ChildNodes)
            {
                switch (child)
                {
                    case Element element:
                        var tagName = element.TagName;

                        builder.EnsureCapacity(builder.Length + 2 * tagName.Length + 5);
                        builder.Append('<').Append(tagName);
                        foreach (var attr in element.AttributeList)
                            builder.Append($" {attr.Name}=\"{EscapeString(attr.Value, true)}\"");
                        builder.Append('>').Append(SerializeHtmlFragment(element));
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

            return builder.ToString();
        }

        public string OuterHtml
        {
            get { return null; }
            set { }
        }

        #endregion
    }
}
