using System.Collections.Generic;

namespace AppToolkit.Html
{
    public class Trie<TValue> : TrieNode<TValue>
    {
        public void AddWord(string word, TValue value)
        {
            TrieNode<TValue> current = this;
            foreach (var c in word)
            {
                var next = current[c];
                if (next == null)
                {
                    next = new TrieNode<TValue>();
                    current[c] = next;
                }
                current = next;
            }
            current.Value = value;
        }
    }

    public class TrieNode<TValue>
    {
        public TValue Value { get; internal set; }

        public bool IsTerminal => edge.Count == 0;

        Dictionary<char, TrieNode<TValue>> edge = new Dictionary<char, TrieNode<TValue>>();
        public TrieNode<TValue> this[char c]
        {
            get
            {
                TrieNode<TValue> v;
                if (edge.TryGetValue(c, out v))
                    return v;
                return null;
            }
            set { edge[c] = value; }
        }

        internal TrieNode() { }
    }
}
