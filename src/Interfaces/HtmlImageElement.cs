using System;

namespace AppToolkit.Html.Interfaces
{
    public class HtmlImageElement : HtmlElement
    {
        public const string Name = "img";

        internal HtmlImageElement(Document nodeDocument, string prefix = null)
            : base(Name, nodeDocument, prefix)
        {
        }

        public string Alt
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string Src
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string SrcSet
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string Sizes
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string CrossOrigin
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public string UseMap
        {
            get { return GetReflectedAttribute(); }
            set { SetReflectedAttribute(value); }
        }
        public bool IsMap
        {
            get { return bool.Parse(GetReflectedAttribute()); }
            set { SetReflectedAttribute(value.ToString()); }
        }
        public uint Width
        {
            get { throw new NotImplementedException(); }
            set { SetReflectedAttribute(value.ToString()); }
        }
        public uint Height
        {
            get { throw new NotImplementedException(); }
            set { SetReflectedAttribute(value.ToString()); }
        }
        public uint NaturalWidth { get { throw new NotImplementedException(); } }
        public uint NaturalHeight { get { throw new NotImplementedException(); } }
        public bool Complete
        {
            get
            {
                if (SrcSet == null && string.IsNullOrEmpty(Src))
                    return true;

                throw new NotImplementedException();
            }
        }
        public uint CurrentSrc { get { throw new NotImplementedException(); } }
        public string RefererPolicy
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
