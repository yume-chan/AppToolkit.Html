using System.Collections.Generic;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public class Text : CharacterData
    {
        public override NodeType NodeType => NodeType.Text;
        public override string NodeName => "#text";

        /// <summary>
        /// Returns a new <see cref="Text"/> node whose <see cref="CharacterData.Data"/> is data. 
        /// </summary>
        /// <param name="data">The data.</param>
        public Text(string data) { Data = data; }
        /// <summary>
        /// Splits <see cref="CharacterData.Data"/> at the given offset and returns the remainder as <see cref="Text"/> node. 
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The remainder as <see cref="Text"/> node. </returns>
        public Text SplitText(uint offset)
        {
            if (offset > Length)
                throw new DomException("IndexSizeError");

            var count = Length - offset;
            var newData = SubstringData(offset, count);
            var newNode = new Text(newData) { ownerDocument = ownerDocument };
            if (ParentNode != null)
            {
                ParentNode.InsertBefore(newNode, NextSibling);
            }

            ReplaceData(offset, count, string.Empty);

            if (ParentNode == null)
            {

            }

            return newNode;
        }
        /// <summary>
        /// Returns the combined data of all direct <see cref="Text"/> node siblings. 
        /// </summary>
        public string WholeText
        {
            get
            {
                var nodes = new List<Text>();
                var text = this;
                while ((text = text.PreviousSibling as Text) != null)
                    nodes.Add(text);
                nodes.Reverse();

                text = this;
                while ((text = text.NextSibling as Text) != null)
                    nodes.Add(text);

                return string.Concat(nodes.Select(x => x.Data));
            }
        }

        internal override Node CloneOverride() => new Text(Data);

        public override string ToString() => Data;
    }

}
