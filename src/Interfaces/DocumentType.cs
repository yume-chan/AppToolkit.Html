namespace AppToolkit.Html.Interfaces
{
    public class DocumentType : Node
    {
        public override NodeType NodeType => NodeType.DocumentType;
        public override string NodeName => Name;

        public string Name { get; internal set; }
        public string PublicId { get; internal set; }
        public string SystemId { get; internal set; }

        internal override Node CloneOverride() => new DocumentType()
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
    }
}
