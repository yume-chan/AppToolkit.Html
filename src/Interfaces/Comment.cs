namespace AppToolkit.Html.Interfaces
{
    public class Comment : CharacterData
    {
        /// <summary>
        /// Returns a new <see cref="Comment"/> node whose <see cref="CharacterData.Data"/> is <paramref name="data"/>. 
        /// </summary>
        /// <param name="data"></param>
        public Comment(string data = "")
            : base(GetGlobalDocument())
        {
            Data = data;
        }

        internal Comment(string data, Document nodeDocument)
            : base(nodeDocument)
        {
            Data = data;
        }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.Comment;
        /// <summary>
        /// Returns a string appropriate for the type of <see cref="Node"/>.
        /// </summary>
        public override string NodeName => "#comment";

        internal override Node CloneOverride() => new Comment(Data, OwnerDocument);
        protected override bool IsEqualNodeOverride(Node other) => Data == ((Comment)other).Data;

        #endregion
    }
}
