namespace AppToolkit.Html.Interfaces
{
    public abstract class ProcessingInstruction : CharacterData
    {
        public abstract string Target { get; }

        protected override bool IsEqualNodeOverride(Node other) => Data == ((ProcessingInstruction)other).Data;
    }
}
