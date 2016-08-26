namespace AppToolkit.Html.Interfaces
{
    public abstract class CharacterData : Node
    {
        private string data = string.Empty;
        public string Data
        {
            get { return data; }
            set
            {
                if (value == null)
                    value = string.Empty;

                ReplaceData(0, Length, value);
            }
        }
        public uint Length => (uint)Data.Length;
        public string SubstringData(uint offset, uint count) => Data.Substring((int)offset, (int)count);
        public void AppendData(string data) => ReplaceData(Length, 0, data);
        public void InsertData(uint offset, string data) => ReplaceData(offset, 0, data);
        public void DeleteData(uint offset, uint count) => ReplaceData(offset, count, string.Empty);
        public void ReplaceData(uint offset, uint count, string data)
        {
            if (offset > Length)
                throw new DomException("IndexSizeError");

            if (offset + count > Length)
                count = Length - offset;

            this.data = this.data.Insert((int)offset, data);
            this.data = this.data.Remove((int)offset + data.Length, (int)count);
        }

        public override string NodeValue
        {
            get { return Data; }
            set { Data = value; }
        }
        public override string TextContent
        {
            get { return Data; }
            set { Data = value; }
        }

        internal override string LookupPrefixOverride(string @namespace) => ParentElement?.LookupPrefixOverride(@namespace);
        internal override string LookupNamespaceUriOverride(string prefix) => ParentElement?.LookupNamespaceUriOverride(prefix);
    }
}
