using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public class DomTokenList : IEnumerable<string>
    {
        private readonly Element Owner;
        private readonly string AttributeName;
        private readonly List<string> InnerList = new List<string>();

        internal DomTokenList(Element owner, string attribute)
        {
            Owner = owner;
            AttributeName = attribute;

            Change();
        }

        public uint Length => (uint)InnerList.Count;
        public string this[uint index] => InnerList[(int)index];
        public bool Contains(string token) => InnerList.Contains(token);

        internal void Change()
        {
            InnerList.Clear();

            var value = Owner.GetAttribute(AttributeName);
            if (value == null)
                return;

            foreach (var item in value.Split(' '))
                if (!InnerList.Contains(item))
                    InnerList.Add(item);
        }

        private string Serialize() => string.Join(" ", InnerList);
        private void Update() => Owner.SetAttribute(AttributeName, Serialize());

        public void Add(params string[] tokens)
        {
            foreach (var item in tokens)
            {
                if (string.IsNullOrEmpty(item))
                    throw new DomException(DomExceptionCode.SyntaxError);

                if (item.Contains(' '))
                    throw new DomException(DomExceptionCode.InvalidCharacterError);

                if (!InnerList.Contains(item))
                    InnerList.Add(item);
            }

            Update();
        }
        public void Remove(params string[] tokens)
        {
            foreach (var item in tokens)
            {
                if (string.IsNullOrEmpty(item))
                    throw new DomException(DomExceptionCode.SyntaxError);

                if (item.Contains(' '))
                    throw new DomException(DomExceptionCode.InvalidCharacterError);

                InnerList.Remove(item);
            }

            Update();
        }
        public bool Toggle(string token, bool? force = null)
        {
            if (force == true || !InnerList.Contains(token))
            {
                Add(token);
                return true;
            }
            else
            {
                Remove(token);
                return false;
            }
        }
        public void Replace(string token, string newToken)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newToken))
                throw new DomException(DomExceptionCode.SyntaxError);

            if (token.Contains(' ') || newToken.Contains(' '))
                throw new DomException(DomExceptionCode.InvalidCharacterError);

            if (!InnerList.Contains(token))
                return;

            InnerList.Remove(token);
            InnerList.Add(newToken);

            Update();
        }

        public IEnumerator<string> GetEnumerator() => InnerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => InnerList.GetEnumerator();
    }
}
