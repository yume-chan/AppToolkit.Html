using AppToolkit.Html.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AppToolkit.Html
{
    public static class Html2Xaml
    {
        public static DependencyProperty BaseUriPorperty { get; } = DependencyProperty.RegisterAttached("BaseUri", typeof(Uri), typeof(Html2Xaml), new PropertyMetadata(null));
        public static Uri GetBaseUri(this FrameworkElement @this)
        {
            return (Uri)@this.GetValue(BaseUriPorperty);
        }
        public static void SetBaseUri(this FrameworkElement @this, Uri value)
        {
            @this.SetValue(BaseUriPorperty, value);
        }

        public static DependencyProperty HtmlProperty { get; } = DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Html2Xaml), new PropertyMetadata(null, onHtmlChanged));
        private static void onHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = d as RichTextBlock;
            owner.Blocks.Clear();

            var newValue = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(newValue))
                return;

            var html = HtmlParser.Parse(newValue);

            var paragraph = new Paragraph();
            CreateChildren(paragraph.Inlines, html.Body, owner.GetBaseUri(), owner.Foreground);
            owner.Blocks.Add(paragraph);
        }
        public static string GetHtml(this FrameworkElement @this)
        {
            return (string)@this.GetValue(HtmlProperty);
        }
        public static void SetHtml(this FrameworkElement @this, string value)
        {
            @this.SetValue(HtmlProperty, value);
        }

        private static Image CreateImage(HtmlElement image, Uri baseUri)
        {
            var src = image.GetAttribute("src");
            Uri uri;
            if (!Uri.TryCreate(src, UriKind.Absolute, out uri) &&
                baseUri == null || !Uri.TryCreate(baseUri, src, out uri))
                return null;

            if (!uri.Scheme.StartsWith("http"))
                return null;

            var result = new Image() { Source = new BitmapImage(uri) };
            if (image.HasAttribute("width"))
                result.Width = double.Parse(image.GetAttribute("width"));
            if (image.HasAttribute("height"))
                result.Height = double.Parse(image.GetAttribute("height"));
            if (double.IsNaN(result.Width) && double.IsNaN(result.Height))
                result.Stretch = Stretch.None;

            return result;
        }

        private static void CreateElement(InlineCollection parent, Node node, Uri baseUri, Brush foreground)
        {
            switch (node.NodeType)
            {
                case NodeType.Element:
                    {
                        var element = node as Element;
                        switch (element.TagName)
                        {
                            case "a":
                                {
                                    var href = element.GetAttribute("href");
                                    if (href == null)
                                        break;

                                    Uri uri;
                                    if (!Uri.TryCreate(href, UriKind.Absolute, out uri) &&
                                        baseUri == null || !Uri.TryCreate(baseUri, href, out uri))
                                        return;

                                    if (!uri.Scheme.StartsWith("http"))
                                        return;

                                    parent.Add(new Run() { Text = " " });

                                    var hyperlink = new Hyperlink() { Foreground = foreground };
                                    hyperlink.Click += (s, e) => { var t = Launcher.LaunchUriAsync(uri); };

                                    foreach (var node2 in element.ChildNodes)
                                    {
                                        switch (node2.NodeType)
                                        {
                                            case NodeType.Text:
                                                CreateElement(hyperlink.Inlines, node2, baseUri, foreground);
                                                break;
                                            case NodeType.Element:
                                                {
                                                    var element2 = node2 as Element;
                                                    switch (element2.TagName)
                                                    {
                                                        case "img":
                                                            {
                                                                parent.Add(hyperlink);

                                                                var hyperlinkButton = new HyperlinkButton();
                                                                hyperlinkButton.Content = CreateImage(element2 as HtmlElement, baseUri);
                                                                hyperlinkButton.Click += (s, e) => { var t = Launcher.LaunchUriAsync(uri); };
                                                                parent.Add(new InlineUIContainer() { Child = hyperlinkButton });

                                                                hyperlink = new Hyperlink() { Foreground = foreground };
                                                                hyperlink.Click += (s, e) => { var t = Launcher.LaunchUriAsync(uri); };
                                                            }
                                                            break;
                                                        default:
                                                            CreateElement(hyperlink.Inlines, node2, baseUri, foreground);
                                                            break;
                                                    }
                                                }
                                                break;
                                        }
                                    }

                                    if (hyperlink.Inlines.Count != 0)
                                        parent.Add(hyperlink);

                                    parent.Add(new Run() { Text = " " });
                                }
                                break;
                            case "img":
                                {
                                    var image = CreateImage(element as HtmlElement, baseUri);
                                    if (image != null)
                                        parent.Add(new InlineUIContainer() { Child = image });
                                }
                                break;
                            case "strong":
                            case "b":
                                {
                                    var span = new Span() { FontWeight = FontWeights.Bold };
                                    CreateChildren(span.Inlines, element as HtmlElement, baseUri, foreground);
                                    parent.Add(span);
                                }
                                break;
                            case "font":
                            case "div":
                            case "p":
                                {
                                    var span = new Span();
                                    foreach (var s in ParseStyle(element.GetAttribute("style")))
                                        switch (s.Key)
                                        {
                                            case "font-size":
                                                {
                                                    var value = s.Value;

                                                    double fontSize;
                                                    if (value.EndsWith("px"))
                                                        fontSize = double.Parse(value.Remove(value.Length - 2));
                                                    else if (value.EndsWith("%"))
                                                        fontSize = 14 * double.Parse(value.Remove(value.Length - 1)) / 100;
                                                    else
                                                        fontSize = 14 * double.Parse(value);

                                                    span.FontSize = fontSize;
                                                }
                                                break;
                                            case "font-weight":
                                                switch (s.Value)
                                                {
                                                    case "bold":
                                                        span.FontWeight = FontWeights.Bold;
                                                        break;
                                                }
                                                break;
                                        }

                                    CreateChildren(span.Inlines, element, baseUri, foreground);
                                    parent.Add(span);
                                }
                                break;
                            case "br":
                                break;
                            case "hr":
                                parent.Add(new LineBreak());
                                parent.Add(new InlineUIContainer() { Child = new Border() { BorderThickness = new Thickness(0, 1, 0, 0), BorderBrush = new SolidColorBrush(Colors.Black), Height = 1, Width = 800 } });
                                parent.Add(new LineBreak());
                                break;
                            case "iframe": // Ignore
                                break;
#if DEBUG
                            default:
                                Debug.WriteLine($"Ignore unknown tag {element.TagName}");
                                break;
#endif
                        }
                    }
                    break;
                case NodeType.Text:
                    parent.Add(new Run() { Text = (node as Text).Data });
                    break;
            }
        }

        private static void CreateChildren(InlineCollection parent, Element node, Uri baseUri, Brush foreground)
        {
            foreach (var child in node.ChildNodes)
                CreateElement(parent, child, baseUri, foreground);
        }

        private enum StyleParserState
        {
            Name,
            Value
        }

        static Dictionary<string, string> ParseStyle(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new Dictionary<string, string>();

            var result = new Dictionary<string, string>();

            var state = StyleParserState.Name;
            var builder = new StringBuilder(10);

            string currentName = null;

            foreach (var c in value)
            {
                switch (c)
                {
                    case ':':
                        if (state == StyleParserState.Name)
                        {
                            currentName = builder.TakeAndClear();
                            if (currentName == "")
                                throw new FormatException();

                            state = StyleParserState.Value;
                        }
                        else
                            builder.Append(c);
                        break;
                    case ';':
                        if (state == StyleParserState.Name)
                            throw new FormatException();
                        else
                        {
                            result.Add(currentName, builder.TakeAndClear());
                            state = StyleParserState.Name;
                        }
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            if (state == StyleParserState.Value)
                result.Add(currentName, builder.TakeAndClear());

            return result;
        }

    }
}
