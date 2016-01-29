using System;
using System.Collections.Generic;

namespace AppToolkit.Html.Interfaces
{
    public enum NodeType : ushort
    {
        /// <summary>
        /// node is an <see cref="Interfaces.Element"/>. 
        /// </summary>
        Element = 1,
        /// <summary>
        /// node is a <see cref="Interfaces.Text"/> node. 
        /// </summary>
        Text = 3,
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

        internal Document ownerDocument;

        /// <summary>
        /// Returns the node document. 
        /// </summary>
        public Document OwnerDocument
        {
            get
            {
                if (this is Document)
                    return null;
                else
                    return ownerDocument;
            }
        }
        /// <summary>
        /// Returns the parent. 
        /// </summary>
        public Node ParentNode { get; }
        /// <summary>
        /// Returns the parent element. 
        /// </summary>
        public Element ParentElement { get; }
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
        public Node FirstChild => ChildNodes.Length > 0 ? ChildNodes[0] : null;
        /// <summary>
        /// Returns the last child. 
        /// </summary>
        public Node LastChild => ChildNodes.Length > 0 ? ChildNodes[ChildNodes.Length - 1] : null;
        /// <summary>
        /// Returns the previous sibling. 
        /// </summary>
        public Node PreviousSibling
        {
            get
            {
                if (ParentNode == null)
                    return null;

                var index = (uint)ParentNode.ChildNodes.innerList.IndexOf(this);
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

                var index = (uint)ParentNode.ChildNodes.innerList.IndexOf(this);
                if (index < ParentNode.ChildNodes.Length - 2)
                    return ParentNode.ChildNodes[index + 1];
                else
                    return null;
            }
        }

        public virtual string NodeValue
        {
            get { return null; }
            set { }
        }
        public virtual string TextContent
        {
            get { return null; }
            set { }
        }
        /// <summary>
        /// Removes empty Text nodes and concatenates the data of remaining contiguous Text nodes into the first of their nodes. 
        /// </summary>
        public virtual void Normalize() { }

        internal abstract Node CloneOverride();
        internal Node Clone(Document document = null, bool cloneChildren = false)
        {
            if (document == null)
                document = ownerDocument;

            var copy = CloneOverride();
            if (!(copy is Document))
                copy.ownerDocument = document;

            foreach (var child in ChildNodes)
                copy.AppendChild(child.Clone(document, cloneChildren));

            return copy;
        }
        /// <summary>
        /// Returns a copy of node. 
        /// </summary>
        /// <param name="deep">If <c>true</c>, the copy also includes the <see cref="Node"/>'s descendants.</param>
        /// <returns></returns>
        public Node CloneNode(bool deep = false) => Clone(null, deep);
        /// <summary>
        /// Returns whether node and other have the same properties. 
        /// </summary>
        /// <param name="other">The other node.</param>
        /// <returns></returns>
        public abstract bool IsEqualNode(Node other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is Node)
                return IsEqualNode(obj as Node);

            return false;
        }

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

            if (ChildNodes.innerList.Contains(other))
                return true;

            foreach (var child in ChildNodes)
                if (child.Contains(other))
                    return true;

            return false;
        }

        internal abstract string LookupPrefixOverride(string @namespace);
        public string LookupPrefix(string @namespace)
        {
            if (@namespace == null)
                return null;

            return LookupPrefixOverride(@namespace);
        }
        internal abstract string LookupNamespaceUriOverride(string prefix);
        public string LookupNamespaceUri(string prefix)
        {
            if (prefix == null)
                prefix = string.Empty;

            return LookupNamespaceUriOverride(prefix);
        }
        public bool IsDefaultNamespace(string @namespace) => @namespace == LookupNamespaceUri(null);

        internal void Remove(Node child, bool suppressObservers = false)
        {
            var index = ChildNodes.innerList.IndexOf(child);
            var oldPreviousSibling = child.PreviousSibling;

            if (!suppressObservers)
            {

            }

            ChildNodes.innerList.RemoveAt(index);
        }

        internal void Insert(Node node, Node child, bool suppressObservers = false)
        {
            var count = 1U;
            if (node is DocumentFragment)
                count = node.ChildNodes.Length;

            if (child != null)
            {

            }

            var nodes = new List<Node>();
            if (node is DocumentFragment)
                foreach (var item in node.ChildNodes)
                    nodes.Add(item);
            else
                nodes.Add(node);

            if (node is DocumentFragment)
            {

                while (node.HasChildNodes())
                    node.Remove(node.ChildNodes[0], true);
            }

            if (!suppressObservers)
            {

            }

            foreach (var item in nodes)
            {
                if (child == null)
                    ChildNodes.innerList.Add(item);
                else
                    ChildNodes.innerList.Insert(ChildNodes.innerList.IndexOf(child), item);
            }
        }

        internal void ReplaceAll(Node node)
        {
            if (node != null)
                ownerDocument.AdoptNode(node);

            var removedNodes = new List<Node>(ChildNodes.innerList);
            var addedNodes = new List<Node>();
            if (node is DocumentFragment)
                foreach (var child in node.ChildNodes)
                    addedNodes.Add(child);
            else
                addedNodes.Add(node);

            while (HasChildNodes())
                Remove(ChildNodes[0], true);

            if (node != null)
                Insert(node, null, true);
        }

        public Node InsertBefore(Node node, Node child)
        {
            if (NodeType != NodeType.Document &&
                NodeType != NodeType.DocumentFragment &&
                NodeType != NodeType.Element)
                throw new DomException("HierarchyRequestError");

            var parent = this;
            do
            {
                if (node == parent)
                    throw new DomException("HierarchyRequestError");
            }
            while ((parent = parent.ParentNode) != null);

            if (child != null && child.ParentNode != this)
                throw new DomException("NotFoundError");

            if (!(node is DocumentFragment) &&
                !(node is DocumentType) &&
                !(node is Element) &&
                !(node is Text) &&
                !(node is ProcessingInstruction) &&
                !(node is Comment))
                throw new DomException("HierarchyRequestError");

            if ((node is Text && this is Document) ||
                (node is DocumentType && !(this is Document)))
                throw new DomException("HierarchyRequestError");

            if (this is Document)
            {

            }

            if (child == node)
                child = node.NextSibling;

            ownerDocument.AdoptNode(node);

            Insert(node, child);

            return node;
        }
        public Node AppendChild(Node node) => InsertBefore(node, null);
        public Node ReplaceChild(Node node, Node child) { throw new NotImplementedException(); }
        public Node RemoveChild(Node child)
        {
            if (child.ParentNode != this)
                throw new DomException("NotFoundError");

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

    public abstract class Node<T> : Node, IEquatable<T> where T : Node
    {
        public virtual bool Equals(T other)
        {
            if (ChildNodes.Length != other.ChildNodes.Length)
                return false;

            for (var i = 0U; i < ChildNodes.Length; i++)
                if (!ChildNodes[i].IsEqualNode(other.ChildNodes[i]))
                    return false;

            return true;
        }

        public override sealed bool IsEqualNode(Node other)
        {
            if (other == null)
                return false;
            if (NodeType != other.NodeType)
                return false;

            return Equals(other as T);
        }
    }
}
