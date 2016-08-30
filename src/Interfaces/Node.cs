using System;
using System.Collections.Generic;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public enum NodeType : ushort
    {
        /// <summary>
        /// node is an <see cref="Interfaces.Element"/>. 
        /// </summary>
        Element = 1,
        /// <summary>
        /// node is a <see cref="Interfaces.Attr"/> node. 
        /// </summary>
        Attribute,
        /// <summary>
        /// node is a <see cref="Interfaces.Text"/> node. 
        /// </summary>
        Text,
        /// <summary>
        /// node is a <see cref="Interfaces.ProcessingInstruction"/> node. 
        /// </summary>
        ProcessingInstruction = 7,
        /// <summary>
        /// node is an <see cref="Interfaces.Comment"/> node. 
        /// </summary>
        Comment,
        /// <summary>
        /// node is an <see cref="Interfaces.Document"/>. 
        /// </summary>
        Document,
        /// <summary>
        /// node is an <see cref="Interfaces.DocumentType"/>. 
        /// </summary>
        DocumentType,
        /// <summary>
        /// node is an <see cref="Interfaces.DocumentFragment"/>. 
        /// </summary>
        DocumentFragment
    }

    public abstract class Node : EventTarget
    {
        internal static Document GetGlobalDocument() => null;

        public Node(Document ownerDocument)
        {
            OwnerDocument = ownerDocument;
        }

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public abstract NodeType NodeType { get; }
        /// <summary>
        /// Returns a string appropriate for the type of <see cref="Node"/>.
        /// </summary>
        public abstract string NodeName { get; }

        /// <summary>
        /// Returns the base URL. 
        /// </summary>
        public string BaseUri { get; }

        /// <summary>
        /// Returns the node document. 
        /// </summary>
        public Document OwnerDocument { get; internal set; }
        /// <summary>
        /// Returns the parent. 
        /// </summary>
        public Node ParentNode { get; internal set; }
        /// <summary>
        /// Returns the parent element. 
        /// </summary>
        public Element ParentElement => ParentNode as Element;
        /// <summary>
        /// Returns whether <see cref="Node"/> has children. 
        /// </summary>
        /// <returns></returns>
        public bool HasChildNodes() => ChildNodes.Length > 0;
        /// <summary>
        /// Returns the children. 
        /// </summary>
        public NodeList ChildNodes { get; } = new NodeList();
        /// <summary>
        /// Returns the first child. 
        /// </summary>
        public Node FirstChild => ChildNodes.FirstOrDefault();
        /// <summary>
        /// Returns the last child. 
        /// </summary>
        public Node LastChild => ChildNodes.LastOrDefault();
        /// <summary>
        /// Returns the previous sibling. 
        /// </summary>
        public Node PreviousSibling
        {
            get
            {
                if (ParentNode == null)
                    return null;

                // `index` will never be -1
                var index = (uint)ParentNode.ChildNodes.IndexOf(this);
                if (index > 0)
                    return ParentNode.ChildNodes[index - 1];
                else
                    return null;
            }
        }
        /// <summary>
        /// Returns the next sibling. 
        /// </summary>
        public Node NextSibling
        {
            get
            {
                if (ParentNode == null)
                    return null;

                // `index` will never be -1
                var index = (uint)ParentNode.ChildNodes.IndexOf(this);
                if (index < ParentNode.ChildNodes.Length - 2)
                    return ParentNode.ChildNodes[index + 1];
                else
                    return null;
            }
        }

        public virtual string NodeValue { get { return null; } set { } }
        public virtual string TextContent { get { return null; } set { } }
        /// <summary>
        /// Removes empty <see cref="Text"/> nodes and concatenates the data of
        /// remaining contiguous <see cref="Text"/> nodes into the first of their nodes. 
        /// </summary>
        public void Normalize() { throw new NotImplementedException(); }

        internal abstract Node CloneOverride();
        /// <summary>
        /// Returns a copy of node. 
        /// </summary>
        /// <param name="deep">If <c>true</c>, the copy also includes the <see cref="Node"/>'s descendants.</param>
        /// <returns></returns>
        public Node CloneNode(bool deep = false)
        {
            var copy = CloneOverride();

            if (deep)
                foreach (var child in ChildNodes)
                    copy.AppendChild(child.CloneNode(deep));

            return copy;
        }
        protected virtual bool IsEqualNodeOverride(Node node) => true;
        /// <summary>
        /// Returns whether node and other have the same properties. 
        /// </summary>
        /// <param name="node">The other node.</param>
        /// <returns></returns>
        public bool IsEqualNode(Node node)
        {
            if (ReferenceEquals(node, null))
                return false;
            if (ReferenceEquals(this, node))
                return true;

            if (NodeType != node.NodeType)
                return false;

            if (!IsEqualNodeOverride(node))
                return false;

            if (ChildNodes.Length != node.ChildNodes.Length)
                return false;

            if (!ChildNodes.SequenceEqual(node.ChildNodes))
                return false;

            return true;
        }

        public override bool Equals(object obj) => IsEqualNode(obj as Node);

        /// <summary>
        /// Returns a bitmask indicating the position of other relative to node.
        /// </summary>
        /// <param name="other">The other node.</param>
        /// <returns></returns>
        public DocumentPosition CompareDocumentPosition(Node other) { throw new NotImplementedException(); }
        /// <summary>
        /// Returns <c>true</c> if other is an inclusive descendant of <see cref="Node"/>, and <c>fase</c> otherwise. 
        /// </summary>
        /// <param name="other">The other node.</param>
        /// <returns><c>true</c> if other is an inclusive descendant of <see cref="Node"/>, and <c>fase</c> otherwise. </returns>
        public bool Contains(Node other)
        {
            if (other == null)
                return false;

            foreach (var child in ChildNodes)
            {
                if (child.IsEqualNode(other))
                    return true;

                if (child.Contains(other))
                    return true;
            }

            return false;
        }

        internal virtual string LookupPrefixOverride(string @namespace) => ParentElement?.LookupPrefixOverride(@namespace);
        public string LookupPrefix(string @namespace)
        {
            if (@namespace == null)
                return null;

            return LookupPrefixOverride(@namespace);
        }
        internal virtual string LookupNamespaceUriOverride(string prefix) => ParentElement?.LookupNamespaceUriOverride(prefix);
        public string LookupNamespaceUri(string prefix)
        {
            if (prefix == null)
                prefix = string.Empty;

            return LookupNamespaceUriOverride(prefix);
        }
        public bool IsDefaultNamespace(string @namespace) => @namespace == LookupNamespaceUri(null);

        internal void Remove(Node child, bool suppressObservers = false)
        {
            var index = ChildNodes.IndexOf(child);
            var oldPreviousSibling = child.PreviousSibling;

            if (!suppressObservers)
            {

            }

            child.ParentNode = null;
            ChildNodes.RemoveAt(index);
        }

        internal void Insert(Node node, int index, bool suppressObservers = false)
        {
            var addedNodes = new List<Node>();

            if (node is DocumentFragment)
            {
                addedNodes.Capacity = node.ChildNodes.Count;

                while (node.HasChildNodes())
                {
                    var item = node.ChildNodes[0];
                    node.Remove(item, true);

                    addedNodes.Add(item);
                    item.ParentNode = this;

                    if (index == -1)
                        ChildNodes.Add(item);
                    else
                        ChildNodes.Insert(index++, item);
                }
            }
            else
            {
                addedNodes.Add(node);
                node.ParentNode = this;

                if (index == -1)
                    ChildNodes.Add(node);
                else
                    ChildNodes.Insert(index++, node);
            }
        }

        internal void ReplaceAll(Node node)
        {
            if (node != null)
                OwnerDocument.AdoptNode(node);

            var removedNodes = new List<Node>(ChildNodes);
            var addedNodes = new List<Node>();
            if (node is DocumentFragment)
                foreach (var child in node.ChildNodes)
                    addedNodes.Add(child);
            else
                addedNodes.Add(node);

            while (HasChildNodes())
                Remove(ChildNodes[0], true);

            if (node != null)
                Insert(node, -1, true);
        }

        public Node InsertBefore(Node node, Node child)
        {
            switch (NodeType)
            {
                case NodeType.Document:
                case NodeType.DocumentFragment:
                case NodeType.Element:
                    break;
                default:
                    throw new DomException(DomExceptionCode.HierarchyRequestError);
            }

            var parent = this;
            do
            {
                if (node == parent)
                    throw new DomException(DomExceptionCode.HierarchyRequestError);
            }
            while ((parent = parent.ParentNode) != null);

            if (child != null && child.ParentNode != this)
                throw new DomException(DomExceptionCode.NotFoundError);

            switch (node.NodeType)
            {
                case NodeType.DocumentFragment:
                    if (NodeType == NodeType.Document)
                    {
                        var fragment = (DocumentFragment)node;

                        if (fragment.ChildElementCount > 1)
                            throw new DomException(DomExceptionCode.HierarchyRequestError);

                        foreach (var item in fragment.ChildNodes)
                            if (item.NodeType == NodeType.Text)
                                throw new DomException(DomExceptionCode.HierarchyRequestError);

                        if (fragment.ChildElementCount == 1)
                        {
                            if (OwnerDocument.ChildElementCount != 0)
                                throw new DomException(DomExceptionCode.HierarchyRequestError);

                            if (child?.NodeType == NodeType.DocumentType)
                                throw new DomException(DomExceptionCode.HierarchyRequestError);

                            if (child != null)
                            {
                                var iterator = OwnerDocument.CreateNodeIterator(child);
                                Node next;
                                while ((next = iterator.NextNode()) != null)
                                    if (next.NodeType == NodeType.DocumentType)
                                        throw new DomException(DomExceptionCode.HierarchyRequestError);
                            }
                        }
                    }
                    break;
                case NodeType.DocumentType:
                    if (NodeType != NodeType.Document)
                        throw new DomException(DomExceptionCode.HierarchyRequestError);

                    var hasElement = false;
                    foreach (var item in ChildNodes)
                    {
                        if (item.NodeType == NodeType.DocumentType)
                            throw new DomException(DomExceptionCode.HierarchyRequestError);

                        if (item.NodeType == NodeType.Element)
                            hasElement = true;

                        if (item == child && hasElement)
                            throw new DomException(DomExceptionCode.HierarchyRequestError);
                    }

                    if (child == null && hasElement)
                        throw new DomException(DomExceptionCode.HierarchyRequestError);
                    break;
                case NodeType.Element:
                    if (NodeType == NodeType.Document)
                    {
                        if (OwnerDocument.ChildElementCount != 0)
                            throw new DomException(DomExceptionCode.HierarchyRequestError);

                        if (child?.NodeType == NodeType.DocumentType)
                            throw new DomException(DomExceptionCode.HierarchyRequestError);

                        if (child != null)
                        {
                            var iterator = OwnerDocument.CreateNodeIterator(child);
                            Node next;
                            while ((next = iterator.NextNode()) != null)
                                if (next.NodeType == NodeType.DocumentType)
                                    throw new DomException(DomExceptionCode.HierarchyRequestError);
                        }
                    }
                    break;
                case NodeType.Text:
                    if (NodeType == NodeType.Document)
                        throw new DomException(DomExceptionCode.HierarchyRequestError);
                    break;
                case NodeType.ProcessingInstruction:
                case NodeType.Comment:
                    break;
                default:
                    throw new DomException(DomExceptionCode.HierarchyRequestError);
            }

            if (child == node)
                child = node.NextSibling;

            OwnerDocument.AdoptNode(node);

            Insert(node, ChildNodes.IndexOf(child));

            return node;
        }
        public Node AppendChild(Node node) => InsertBefore(node, null);
        public Node ReplaceChild(Node node, Node child) { throw new NotImplementedException(); }
        public Node RemoveChild(Node child)
        {
            if (child.ParentNode != this)
                throw new DomException(DomExceptionCode.NotFoundError);

            Remove(child);
            return child;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return NodeName;
        }
    }
}
