using System;
using System.Linq;

namespace AppToolkit.Html.Interfaces
{
    public class HtmlDocument : Document
    {
        internal HtmlDocument(DocumentState state)
            : base(state)
        {
            state.Type = DocumentHtmlType.Html;
        }

        #region Override Node

        public override NodeType NodeType => NodeType.Document;
        public override string NodeName => "#document";

        internal override string LookupPrefixOverride(string @namespace) => DocumentElement?.LookupPrefixOverride(@namespace);
        internal override string LookupNamespaceUriOverride(string prefix) => DocumentElement?.LookupNamespaceUriOverride(prefix);

        internal override Node CloneOverride() => new HtmlDocument(State.Clone());

        #endregion

        #region Extension WHATWG HTML Standard
        // https://html.spec.whatwg.org/multipage/dom.html#the-document-object

        public string Domain { get; }
        public string Referer { get; }
        public string Cookie { get; set; }
        public string LastModified { get; }

        public string Title { get; set; }

        public string Dir { get; set; }
        public HtmlElement Body { get; set; }
        public HtmlHeadElement Head => DocumentElement?.ChildNodes.OfType<HtmlHeadElement>().FirstOrDefault();
        public HtmlCollection Images { get; }
        public HtmlCollection Embeds { get; }
        public HtmlCollection Plugins { get; }
        public HtmlCollection Links { get; }
        public HtmlCollection Forms { get; }
        public HtmlCollection Scripts { get; }

        public NodeList GetElementsByName(string elementName) { throw new NotImplementedException(); }

        Document Open(string type = "text/html", string replace = "") { throw new NotImplementedException(); }
        void Close() { throw new NotImplementedException(); }
        void Write(params string[] text) { throw new NotImplementedException(); }
        void WriteLn(params string[] text) { throw new NotImplementedException(); }

        #endregion
    }
}
