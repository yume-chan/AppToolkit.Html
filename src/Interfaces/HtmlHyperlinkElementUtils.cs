using System;

namespace AppToolkit.Html.Interfaces
{
    public interface HtmlHyperlinkElementUtils
    {
        string Href { get; set; }
    }

    internal class HtmlHyperlinkElementUtilsParentNodeImplementation : HtmlHyperlinkElementUtils
    {
        public Element Owner { get; }

        private Uri Uri;

        public HtmlHyperlinkElementUtilsParentNodeImplementation(Element owner)
        {
            Owner = owner;
        }

        public string Href
        {
            get
            {
                var href = Owner.GetAttribute("href");
                if (href == null)
                    return string.Empty;

                if (!Uri.TryCreate(href, UriKind.Absolute, out Uri) &&
                    !Uri.TryCreate(Owner.OwnerDocument.State.Url, href, out Uri))
                    return href;

                return Uri.ToString();
            }
            set
            {
                Owner.SetAttribute("href", value);
            }
        }
    }
}
