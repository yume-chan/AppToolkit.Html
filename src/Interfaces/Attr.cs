using System;

namespace AppToolkit.Html.Interfaces
{
    public class Attr : Node
    {
        internal Attr(string localName, string value, Element ownerElement, Document ownerDocument)
            : base(ownerDocument)
        {
            LocalName = localName;
            Name = localName;
            Value = value;
            OwnerElement = ownerElement;
        }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.Attribute;
        public override string NodeName
        {
            get
            {
                if (NamespaceUri == null)
                    return LocalName;
                else
                    return $"{Prefix}:{LocalName}";
            }
        }

        public override string NodeValue
        {
            get { return Value; }
            set { throw new NotImplementedException(); }
        }

        public override string TextContent
        {
            get { return Value; }
            set { Value = value; }
        }

        internal override Node CloneOverride() => new Attr(LocalName, Value, OwnerElement, OwnerDocument)
        {
            NamespaceUri = NamespaceUri,
            Prefix = Prefix
        };
        protected override bool IsEqualNodeOverride(Node node)
        {
            var element = (Attr)node;

            if (NamespaceUri != element.NamespaceUri)
                return false;

            if (LocalName != element.LocalName)
                return false;

            if (Value != element.Value)
                return false;

            return true;
        }

        internal override string LookupPrefixOverride(string @namespace) => OwnerElement?.LookupPrefixOverride(@namespace);
        internal override string LookupNamespaceUriOverride(string prefix) => OwnerElement?.LookupNamespaceUriOverride(prefix);

        #endregion

        public string NamespaceUri { get; internal set; }
        public string Prefix { get; internal set; }
        public string LocalName { get; }
        public string Name { get; }

        public string Value { get; set; }

        public Element OwnerElement { get; }
    }
}
