namespace AppToolkit.Html.Interfaces
{
    public enum RangeBoundaryPointCompareMethod
    {
        StartToStart,
        StartToEnd,
        EndToEnd,
        EndToStart
    }

    public abstract class Range
    {
        public Range() { }

        public abstract Node StartContainer { get; }
        public abstract uint StartOffset { get; }
        public abstract Node EndContainer { get; }
        public abstract uint EndOffset { get; }
        public abstract bool Collapsed { get; }
        public abstract Node CommonAncestorContainer { get; }

        public abstract void SetStart(Node node, uint offset);
        public abstract void SetEnd(Node node, uint offset);
        public abstract void SetStartbefore(Node node);
        public abstract void SetStartAfter(Node node);
        public abstract void SetEndBefore(Node node);
        public abstract void SetEndAfter(Node node);
        public abstract void Collapse(bool toStart = false);
        public abstract void SelectNode(Node node);
        public abstract void SelectNodeContents(Node node);

        public abstract ushort CompareBoundaryPoints(RangeBoundaryPointCompareMethod how, Range source);

        public abstract void DeleteContents();
        public abstract DocumentFragment ExtractContents();
        public abstract DocumentFragment CloneContents();
        public abstract void InsertNode(Node node);
        public abstract void SurroundContents(Node newParent);

        public abstract Range CloneRange();
        public abstract void Detach();

        public abstract bool IsPointInRange(Node node, uint offset);
        public abstract short ComparePoint(Node node, uint offset);

        public abstract bool IntersectsNode(Node node);
    }

}
