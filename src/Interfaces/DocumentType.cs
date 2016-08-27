namespace AppToolkit.Html.Interfaces
{
    public class DocumentType : Node
    {
        internal DocumentType(Document nodeDocument)
            : base(nodeDocument)
        { }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.DocumentType;
        /// <summary>
        /// Returns a string appropriate for the type of <see cref="Node"/>.
        /// </summary>
        public override string NodeName => Name;

        internal override Node CloneOverride() => new DocumentType(OwnerDocument)
        {
            Name = Name,
            PublicId = PublicId,
            SystemId = SystemId
        };
        protected override bool IsEqualNodeOverride(Node node)
        {
            var type = (DocumentType)node;

            if (Name != type.Name)
                return false;

            if (PublicId != type.PublicId)
                return false;

            if (SystemId != type.SystemId)
                return false;

            return true;
        }

        internal override string LookupPrefixOverride(string @namespace) => null;
        internal override string LookupNamespaceUriOverride(string prefix) => null;

        #endregion

        public string Name { get; internal set; }
        public string PublicId { get; internal set; }
        public string SystemId { get; internal set; }
    }
}
