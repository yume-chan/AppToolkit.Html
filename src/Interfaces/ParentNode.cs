using System;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    /// <summary>
    /// See also: https://dom.spec.whatwg.org/#interface-parentnode
    /// </summary>
    public interface ParentNode
    {
        /// <summary>
        /// Returns the child <see cref="Element"/>s.
        /// </summary>
        HtmlCollection Children { get; }
        /// <summary>
        /// Returns the first child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        Element FirstElementChild { get; }
        /// <summary>
        /// Returns the last child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        Element LastElementChild { get; }
        /// <summary>
        /// Returns the number of children of context object that are <see cref="Element"/>. 
        /// </summary>
        uint ChildElementCount { get; }

        /// <summary>
        /// Inserts <paramref name="nodes"/> before the first child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        void Prepend(params object[] nodes);
        /// <summary>
        /// Inserts <paramref name="nodes"/> after the last child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        void Append(params object[] nodes);

        /// <summary>
        /// Returns the first element that is a descendant of node that matches <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the first result of running scope-match a selectors string <paramref name="selectors"/> against context object,
        /// if the result is not an empty list, and <code>null</code> otherwise. 
        /// </returns>
        Element QuerySelector(string selectors);
        /// <summary>
        /// Returns all element descendants of node that match <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the static result of running scope-match a selectors string <paramref name="selectors"/> against context object.
        /// </returns>
        NodeList QuerySelectorAll(string selectors);
    }

    internal class ParentNodeImplementation : ParentNode
    {
        public Node Owner { get; }

        public ParentNodeImplementation(Node owner)
        {
            Owner = owner;
            Children = new ChildrenHtmlCollection(owner);
        }

        #region Implement ParentNode

        /// <summary>
        /// Returns the child <see cref="Element"/>s.
        /// </summary>
        public HtmlCollection Children { get; }
        /// <summary>
        /// Returns the first child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        public Element FirstElementChild => Children.FirstOrDefault();
        /// <summary>
        /// Returns the last child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        public Element LastElementChild => Children.LastOrDefault();
        /// <summary>
        /// Returns the number of children of context object that are <see cref="Element"/>. 
        /// </summary>
        public uint ChildElementCount => Children.Length;

        /// <summary>
        /// Inserts <paramref name="nodes"/> before the first child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        public void Prepend(params object[] nodes)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Inserts <paramref name="nodes"/> after the last child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        public void Append(params object[] nodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the first element that is a descendant of node that matches <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the first result of running scope-match a selectors string <paramref name="selectors"/> against context object,
        /// if the result is not an empty list, and <code>null</code> otherwise. 
        /// </returns>
        public Element QuerySelector(string selectors)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns all element descendants of node that match <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the static result of running scope-match a selectors string <paramref name="selectors"/> against context object.
        /// </returns>
        public NodeList QuerySelectorAll(string selectors)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class ParentNodeExample : Node, ParentNode
    {
        #region Override Node

        public override NodeType NodeType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string NodeName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal override Node CloneOverride()
        {
            throw new NotImplementedException();
        }

        internal override string LookupPrefixOverride(string @namespace)
        {
            throw new NotImplementedException();
        }

        internal override string LookupNamespaceUriOverride(string prefix)
        {
            throw new NotImplementedException();
        }

        #endregion

        public ParentNodeExample()
            : base(null)
        {
            ParentNodeImplementation = new ParentNodeImplementation(this);
        }

        #region Implement ParentNode

        private readonly ParentNodeImplementation ParentNodeImplementation;

        /// <summary>
        /// Returns the child <see cref="Element"/>s.
        /// </summary>
        public HtmlCollection Children => ParentNodeImplementation.Children;
        /// <summary>
        /// Returns the first child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        public Element FirstElementChild => ParentNodeImplementation.FirstElementChild;
        /// <summary>
        /// Returns the last child that is an <see cref="Element"/>, and <code>null</code> otherwise.
        /// </summary>
        public Element LastElementChild => ParentNodeImplementation.LastElementChild;
        /// <summary>
        /// Returns the number of children of context object that are <see cref="Element"/>. 
        /// </summary>
        public uint ChildElementCount => ParentNodeImplementation.ChildElementCount;

        /// <summary>
        /// Inserts <paramref name="nodes"/> before the first child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        public void Prepend(params object[] nodes) => ParentNodeImplementation.Prepend(nodes);
        /// <summary>
        /// Inserts <paramref name="nodes"/> after the last child of node, while replacing strings in <paramref name="nodes"/>
        /// with equivalent <see cref="Text"/> nodes.
        /// </summary>
        /// <exception cref="DomException">
        /// Throws a <see cref="DomExceptionCode.HierarchyRequestError"/> if the constraints of the node tree are violated.
        /// </exception>
        public void Append(params object[] nodes) => ParentNodeImplementation.Append(nodes);

        /// <summary>
        /// Returns the first element that is a descendant of node that matches <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the first result of running scope-match a selectors string <paramref name="selectors"/> against context object,
        /// if the result is not an empty list, and <code>null</code> otherwise. 
        /// </returns>
        public Element QuerySelector(string selectors) => ParentNodeImplementation.QuerySelector(selectors);
        /// <summary>
        /// Returns all element descendants of node that match <paramref name="selectors"/>. 
        /// </summary>
        /// <returns>
        /// Returns the static result of running scope-match a selectors string <paramref name="selectors"/> against context object.
        /// </returns>
        public NodeList QuerySelectorAll(string selectors) => ParentNodeImplementation.QuerySelectorAll(selectors);

        #endregion
    }
}
