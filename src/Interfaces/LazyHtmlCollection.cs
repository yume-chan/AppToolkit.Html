using System;
using System.Collections.Generic;

namespace AppToolkit.Html.Interfaces
{
    internal class LazyHtmlCollection : HtmlCollection
    {
        private readonly NodeList Nodes;
        private readonly Func<Element, bool> Matcher;

        internal int EvaluatedCount { get; set; }

        internal void Insert(int index, Element element)
        {
            if (Matcher == null || Matcher(element))
                InnerList.Insert(index, element);
        }

        public LazyHtmlCollection(NodeList nodes, Func<Element, bool> matcher = null)
            : base(new List<Element>())
        {
            nodes.HtmlCollections.Add(new WeakReference<LazyHtmlCollection>(this));

            Nodes = nodes;
            Matcher = matcher;
        }

        public override uint Length
        {
            get
            {
                while (EvaluatedCount < Nodes.Length)
                {
                    if (Nodes[EvaluatedCount] is Element element)
                    {
                        if (Matcher == null || Matcher(element))
                            InnerList.Add(element);
                    }

                    EvaluatedCount++;
                }

                return (uint)InnerList.Count;
            }
        }

        public override Element this[uint index]
        {
            get
            {
                if (index >= int.MaxValue)
                    throw new IndexOutOfRangeException();

                var i = (int)index;

                if (InnerList.Count > i)
                    return InnerList[i];

                while (EvaluatedCount < Nodes.Length)
                {
                    if (Nodes[EvaluatedCount] is Element element)
                    {
                        if (Matcher == null || Matcher(element))
                        {
                            InnerList.Add(element);

                            if (InnerList.Count > i)
                                return element;
                        }
                    }

                    EvaluatedCount++;
                }

                return null;
            }
        }

        public override Element this[string name]
        {
            get
            {
                foreach (var item in InnerList)
                    if (IsNameMatches(item, name))
                        return item;

                while (EvaluatedCount < Nodes.Length)
                {
                    if (Nodes[EvaluatedCount] is Element element)
                    {
                        if (Matcher == null || Matcher(element))
                        {
                            InnerList.Add(element);

                            if (IsNameMatches(element, name))
                                return element;
                        }
                    }

                    EvaluatedCount++;
                }

                return null;
            }
        }

        protected override IEnumerator<Element> GetEnumerator()
        {
            foreach (var item in InnerList)
                yield return item;

            while (EvaluatedCount < Nodes.Length)
            {
                if (Nodes[EvaluatedCount] is Element element)
                {
                    if (Matcher == null || Matcher(element))
                    {
                        InnerList.Add(element);
                        yield return element;
                    }
                }

                EvaluatedCount++;
            }
        }
    }
}
