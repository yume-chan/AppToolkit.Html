using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public class DocumentFragment : Node, ParentNode
    {
        public DocumentFragment()
            : this(GetGlobalDocument())
        { }

        internal DocumentFragment(Document nodeDocument)
            : base(nodeDocument)
        {
            ParentNodeImplementation = new ParentNodeImplementation(this);
        }

        #region Override Node

        /// <summary>
        /// Returns the type of <see cref="Node"/>.
        /// </summary>
        public override NodeType NodeType => NodeType.DocumentFragment;
        public override string NodeName => "#document-fragment";

        public override string TextContent
        {
            get
            {
                return string.Concat(ChildNodes.Select(x => x.TextContent));
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    ReplaceAll(null);
                else
                    ReplaceAll(new Text(value));
            }
        }

        internal override Node CloneOverride() => new DocumentFragment(OwnerDocument);

        internal override string LookupPrefixOverride(string @namespace) => null;
        internal override string LookupNamespaceUriOverride(string prefix) => null;

        #endregion

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
