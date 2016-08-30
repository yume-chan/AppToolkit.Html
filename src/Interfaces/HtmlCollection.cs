using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public class HtmlCollection<T> : IEnumerable<T> where T : Element
    {
        internal static HtmlCollection<T> Empty { get; } = new HtmlCollection<T>(new List<T>());

        internal List<T> InnerList { get; }

        internal HtmlCollection(List<T> innerList)
        {
            InnerList = innerList;
        }

        public virtual uint Length => (uint)InnerList.Count;
        public virtual Element this[uint index] => index < InnerList.Count ? InnerList[(int)index] : null;
        public virtual Element this[string name] => !string.IsNullOrEmpty(name) ? InnerList.FirstOrDefault(x => IsNameMatches(x, name)) : null;

        protected bool IsNameMatches(Element element, string name)
        {
            if (element.Id == name)
                return true;

            if (element.NamespaceUri != HtmlElement.HtmlNamespace)
                return false;

            if (element.GetAttribute("name") == name)
                return true;

            return false;
        }

        protected virtual IEnumerator<T> GetEnumerator() => InnerList.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal HtmlCollection<TOther> Cast<TOther>() where TOther : Element
        {
            if (typeof(TOther) == typeof(HtmlCollection))
                return new HtmlCollection(new List<Element>(InnerList.Cast<Element>())) as HtmlCollection<TOther>;
            else
                return new HtmlCollection<TOther>(new List<TOther>(InnerList.Cast<TOther>()));
        }
    }

    public class HtmlCollection : HtmlCollection<Element>
    {
        internal new static HtmlCollection Empty { get; } = new HtmlCollection(new List<Element>());

        public HtmlCollection(List<Element> innerList)
            : base(innerList)
        { }
    }
}
