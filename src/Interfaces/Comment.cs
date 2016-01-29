namespace AppToolkit.Html.Interfaces
{
    public class Comment : CharacterData
    {
        public override NodeType NodeType => NodeType.Comment;
        public override string NodeName => "#comment";

        public Comment(string data = "")
        {
            Data = data;
        }

        internal override Node CloneOverride() => new Comment(Data);

        public override string ToString()
        {
            return $"<!-- {Data} -->";
        }
    }
}
