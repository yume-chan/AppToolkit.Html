using System;

namespace AppToolkit.Html.Interfaces
{
    public class HtmlAnchorElement : HtmlElement
    {
        public const string Name = "a";

        internal HtmlAnchorElement(Document nodeDocument, string prefix = null)
            : base(Name, nodeDocument, prefix)
        {
            RelList = CreateAttributeTokenList("rel");
        }

        public string Target
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string Download
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string Ping
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string Rel
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public DomTokenList RelList { get; }
        public string HrefLang
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string Type
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }

        public string Text
        {
            get { return TextContent; }
            set { TextContent = value; }
        }

        public string RefererPolicy
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
