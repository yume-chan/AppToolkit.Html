namespace AppToolkit.Html.Interfaces
{
    public class ProcessingInstruction : CharacterData
    {
        internal ProcessingInstruction(string target, string data, Document ownerDocument)
            : base(ownerDocument)
        {
            Target = target;
            Data = data;
        }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.ProcessingInstruction;
        /// <summary>
        /// Returns a string appropriate for the type of <see cref="Node"/>.
        /// </summary>
        public override string NodeName => Target;

        internal override Node CloneOverride() => new ProcessingInstruction(Target, Data, OwnerDocument);
        protected override bool IsEqualNodeOverride(Node other)
        {
            var instruction = (ProcessingInstruction)other;

            if (Target != instruction.Target)
                return false;

            if (Data != instruction.Data)
                return false;

            return true;
        }

        #endregion

        public string Target { get; }
    }
}
