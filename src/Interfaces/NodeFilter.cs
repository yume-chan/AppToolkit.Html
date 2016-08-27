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
        ProcessingInstruction = 0x40,
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
            if (((uint)WhatToShow & 1 << (int)(node.NodeType - 1)) == 0)
                return NodeFilterResult.Skip;
            if (Filter != null)
                return Filter(node);
            return NodeFilterResult.Accept;
        }

        internal Node Traverse(bool next)
        {
            var node = ReferenceNode;

            do
            {
                if (PointerBeforeReferenceNode == next)
                {
                    PointerBeforeReferenceNode = !PointerBeforeReferenceNode;
                }
                else
                {
                    node = next ? node.FirstChild : node.LastChild;
                    if (node != null && FilterNode(node) == NodeFilterResult.Accept)
                        break;

                    node = next ? node.NextSibling : node.PreviousSibling;
                    if (node != null && FilterNode(node) == NodeFilterResult.Accept)
                        break;

                    do
                    {
                        node = node.ParentNode;
                        if (node == null)
                            return null;

                        node = next ? node.NextSibling : node.PreviousSibling;
                        if (node != null)
                            break;
                    }
                    while (true);

                    if (FilterNode(node) == NodeFilterResult.Accept)
                        break;
                }
            }
            while (true);

            ReferenceNode = node;
            return node;
        }

        public Node NextNode() => Traverse(true);

        public Node PreviousNode() => Traverse(false);

        [Obsolete("Its functionality (disabling a NodeIterator object) was removed, but the method itself is preserved for compatibility.", true)]
        public void Detach() { }
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
            if (((uint)WhatToShow & 1 << (int)(node.NodeType - 1)) == 0)
                return NodeFilterResult.Skip;
            if (Filter != null)
                return Filter(node);
            return NodeFilterResult.Accept;
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
