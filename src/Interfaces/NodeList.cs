using System;
using System.Collections;
using System.Collections.Generic;

namespace AppToolkit.Html.Interfaces
{
    public class NodeList : IEnumerable<Node>
    {
        internal readonly List<WeakReference<LazyHtmlCollection>> HtmlCollections = new List<WeakReference<LazyHtmlCollection>>();
        private List<Node> InnerList = new List<Node>();

        internal void Add(Node item) => InnerList.Add(item);

        internal void RemoveAt(int index)
        {
            if (index < 0 || index >= InnerList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var element = InnerList[index] as Element;

            for (var i = 0; i < HtmlCollections.Count;)
            {
                if (!HtmlCollections[i].TryGetTarget(out var collection))
                {
                    HtmlCollections.RemoveAt(i);
                    break;
                }

                if (collection.EvaluatedCount >= index)
                {
                    collection.EvaluatedCount--;

                    if (element != null)
                        collection.InnerList.Remove(element);
                }

                i++;
            }

            InnerList.RemoveAt(index);
        }

        internal bool Contains(Node item) => InnerList.Contains(item);

        internal int IndexOf(Node item) => InnerList.IndexOf(item);

        private void InsertToCollection(int index, Element element, LazyHtmlCollection collection)
        {
            var list = collection.InnerList;

            var i = 0;
            for (var j = 0; j < list.Count; j++)
            {
                var item = list[j];
                while (InnerList[i] != item)
                    i++;

                if (i >= index)
                    collection.Insert(j, element);
            }
        }
        internal void Insert(int index, Node node)
        {
            if (index < 0 || index >= InnerList.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var element = node as Element;

            for (var i = 0; i < HtmlCollections.Count;)
            {
                if (!HtmlCollections[i].TryGetTarget(out var collection))
                {
                    HtmlCollections.RemoveAt(i);
                    break;
                }

                if (collection.EvaluatedCount >= index)
                {
                    collection.EvaluatedCount++;

                    if (element != null)
                        InsertToCollection(index, element, collection);
                }

                i++;
            }

            InnerList.Insert(index, node);
        }

        internal Node this[int index] => InnerList[index];

        /// <summary>
        /// Returns the node with index index from the collection. The nodes are sorted in tree order.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Node this[uint index] => index < InnerList.Count ? InnerList[(int)index] : null;
        /// <summary>
        /// Returns the number of nodes in the collection.
        /// </summary>
        public uint Length => (uint)InnerList.Count;

        internal int Count => InnerList.Count;

        public IEnumerator<Node> GetEnumerator() => InnerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
