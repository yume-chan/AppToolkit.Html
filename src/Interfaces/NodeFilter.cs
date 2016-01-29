using System;

namespace AppToolkit.Html.Interfaces
{
    public enum NodeFilterResult : ushort
    {
        Accept = 1,
        Reject = 2,
        Skip = 3
    }

    public delegate NodeFilterResult NodeFilter(Node node);

    [Flags]
    public enum WhatToShow : uint
    {
        All = 0xFFFFFFFF,
        Element = 0x1,
        Text = 0x4,
        ProcessInstruction = 0x40,
        Comment = 0x80,
        Document = 0x100,
        DocumentType = 0x200,
        DocumentFragment = 0x400
    }

    public class NodeIterator
    {
        public Node Root { get; }
        public Node ReferenceNode { get; private set; }
        public bool PointerBeforeReferenceNode { get; private set; }
        public WhatToShow WhatToShow { get; }
        public NodeFilter Filter { get; }

        internal NodeIterator(Node root, WhatToShow whatToShow, NodeFilter filter)
        {
            Root = root;

            ReferenceNode = root;
            PointerBeforeReferenceNode = true;

            WhatToShow = whatToShow;
            Filter = filter;
        }

        internal NodeFilterResult FilterNode(Node node)
        {
            var n = node.NodeType - 1;
            if (((uint)WhatToShow & (uint)n) == 0)
                return NodeFilterResult.Skip;
            if (Filter == null)
                return NodeFilterResult.Accept;
            return Filter(node);
        }

        internal Node Traverse(bool next)
        {
            throw new NotImplementedException();
        }

        public Node NextNode() => Traverse(true);

        public Node PreviousNode() => Traverse(false);
    }

    public class TreeWalker
    {
        public Node Root { get; }
        public WhatToShow WhatToShow { get; }
        public NodeFilter Filter { get; }

        public Node CurrentNode { get; set; }

        internal TreeWalker(Node root, WhatToShow whatToShow, NodeFilter filter)
        {
            Root = root;
            WhatToShow = whatToShow;
            Filter = filter;
        }

        internal NodeFilterResult FilterNode(Node node)
        {
            var n = node.NodeType - 1;
            if (((uint)WhatToShow & (uint)n) == 0)
                return NodeFilterResult.Skip;
            if (Filter == null)
                return NodeFilterResult.Accept;
            return Filter(node);
        }

        public Node ParentNode()
        {
            if (CurrentNode != Root)
            {
                var node = CurrentNode.ParentNode;
                if (node != null && FilterNode(node) == NodeFilterResult.Accept)
                {
                    CurrentNode = node;
                    return node;
                }
            }

            return null;
        }

        internal Node TraverseChildren(bool firstChild)
        {
            var node = firstChild ? CurrentNode.FirstChild : CurrentNode.LastChild;
            while (node != null)
            {
                switch (FilterNode(node))
                {
                    case NodeFilterResult.Accept:
                        CurrentNode = node;
                        return node;
                    case NodeFilterResult.Reject:
                        while (node != null)
                        {
                            var sibling = firstChild ? node.NextSibling : node.PreviousSibling;
                            if (sibling != null)
                            {
                                node = sibling;
                                break;
                            }

                            node = node.ParentNode;
                            if (node == null || node == Root || node == CurrentNode)
                                return null;
                        }
                        break;
                    case NodeFilterResult.Skip:
                        break;
                    default:
                        break;
                }
            }
            return null;
        }

        public Node FirstChild() => TraverseChildren(true);
        public Node LastChild() => TraverseChildren(false);

        internal Node TraverseSiblings(bool nextSibling)
        {
            var node = CurrentNode;
            if (node == Root)
                return null;

            while (true)
            {
                var sibling = nextSibling ? node.NextSibling : node.PreviousSibling;
                while (sibling != null)
                {
                    node = sibling;

                    var result = FilterNode(node);
                    if (result == NodeFilterResult.Accept)
                    {
                        CurrentNode = node;
                        return node;
                    }

                    sibling = nextSibling ? node.FirstChild : node.LastChild;
                    if (result == NodeFilterResult.Reject || sibling == null)
                        sibling = nextSibling ? node.NextSibling : node.PreviousSibling;
                }

                node = node.ParentNode;
                if (node == null || node == Root)
                    return null;
                if (FilterNode(node) == NodeFilterResult.Accept)
                    return null;
            }
        }

        public Node NextSibling() => TraverseSiblings(true);
        public Node PreviousSibling() => TraverseSiblings(false);

        public Node PreviousNode()
        {
            var node = CurrentNode;
            while (node != Root)
            {
                var sibling = node.PreviousSibling;
                while (sibling != null)
                {
                    node = sibling;
                    var result = FilterNode(node);
                    while (result != NodeFilterResult.Reject && node.HasChildNodes())
                    {
                        node = node.LastChild;
                        result = FilterNode(node);
                    }

                    if (result == NodeFilterResult.Accept)
                    {
                        CurrentNode = node;
                        return node;
                    }

                    sibling = node.PreviousSibling;
                }

                if (node == Root || node.ParentNode == null)
                    return null;

                node = node.ParentNode;

                if (FilterNode(node) == NodeFilterResult.Accept)
                {
                    CurrentNode = node;
                    return node;
                }
            }
            return null;
        }

        public Node NextNode()
        {
            var node = CurrentNode;
            var result = NodeFilterResult.Accept;
            while (true)
            {
                while (result != NodeFilterResult.Reject && node.HasChildNodes())
                {
                    node = node.FirstChild;
                    result = FilterNode(node);
                    if (result == NodeFilterResult.Accept)
                    {
                        CurrentNode = node;
                        return node;
                    }
                }

                return null;
            }
        }
    }

}
