using AppToolkit.Converters;
using AppToolkit.Html.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
        public class RichTextBlockStatus
        {
            public long RequestedThemePropertyToken;

            public long ForegroundToken;
        }

        public static DependencyProperty BaseUriPorperty { get; } = DependencyProperty.RegisterAttached("BaseUri", typeof(Uri), typeof(Html2Xaml), new PropertyMetadata(null));
        public static Uri GetBaseUri(this FrameworkElement @this)
        {
            return (Uri)@this.GetValue(BaseUriPorperty);
        }
        public static void SetBaseUri(this FrameworkElement @this, Uri value)
        {
            @this.SetValue(BaseUriPorperty, value);
        }

        public static DependencyProperty StatusPorperty { get; } = DependencyProperty.RegisterAttached("Status", typeof(RichTextBlockStatus), typeof(Html2Xaml), new PropertyMetadata(null));
        public static RichTextBlockStatus GetStatus(this FrameworkElement @this)
        {
            return (RichTextBlockStatus)@this.GetValue(StatusPorperty);
        }
        public static void SetStatus(this FrameworkElement @this, RichTextBlockStatus value)
        {
            @this.SetValue(StatusPorperty, value);
        }

        public static DependencyProperty HtmlProperty { get; } = DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Html2Xaml), new PropertyMetadata(null, onHtmlChanged));
        private static void onHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (RichTextBlock)d;
            owner.Blocks.Clear();

            var status = owner.GetStatus();
            if (status == null)
            {
                status = new RichTextBlockStatus();
                owner.SetStatus(status);

                owner.Loaded += Owner_Loaded;
                owner.Unloaded += Owner_Unloaded;

                status.RequestedThemePropertyToken = owner.RegisterPropertyChangedCallback(FrameworkElement.RequestedThemeProperty, onRequestedThemeChanged);
                status.ForegroundToken = owner.RegisterPropertyChangedCallback(RichTextBlock.ForegroundProperty, onForegroundChanged);
                owner.SizeChanged += onSizeChanged;
            }

            var newValue = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(newValue))
                return;

            var html = (HtmlDocument)HtmlParser.Parse(newValue, true);

            var paragraph = new Paragraph();
            CreateChildren(paragraph.Inlines, html.Body, owner, owner.GetBaseUri());
            owner.Blocks.Add(paragraph);
        }

        private static void onRequestedThemeChanged(DependencyObject sender, DependencyProperty dp)
        {
            var owner = (RichTextBlock)sender;

            void VisitNodeCollection(InlineCollection collection)
            {
                foreach (var inline in collection)
                {
                    switch (inline)
                    {
                        case Hyperlink hyperlink:
                            hyperlink.Foreground = owner.Foreground;
                            VisitNodeCollection(hyperlink.Inlines);
                            break;
                        case Span span:
                            VisitNodeCollection(span.Inlines);
                            break;
                    }
                }
            }

            foreach (Paragraph paragraph in owner.Blocks)
                VisitNodeCollection(paragraph.Inlines);
        }

        private static void onForegroundChanged(DependencyObject sender, DependencyProperty dp)
        {
            var owner = (RichTextBlock)sender;

            void VisitNodeCollection(InlineCollection collection)
            {
                foreach (var inline in collection)
                {
                    switch (inline)
                    {
                        case InlineUIContainer container when container.Child is Control control:
                            control.RequestedTheme = owner.RequestedTheme;
                            break;
                        case Span span:
                            VisitNodeCollection(span.Inlines);
                            break;
                    }
                }
            }

            foreach (Paragraph paragraph in owner.Blocks)
                VisitNodeCollection(paragraph.Inlines);
        }

        private static void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var owner = (RichTextBlock)sender;

            void VisitNodeCollection(InlineCollection collection)
            {
                foreach (var inline in collection)
                {
                    switch (inline)
                    {
                        case InlineUIContainer container when container.Child is FrameworkElement element:
                            element.MaxWidth = e.NewSize.Width;
                            break;
                        case Span span:
                            VisitNodeCollection(span.Inlines);
                            break;
                    }
                }
            }

            foreach (Paragraph paragraph in owner.Blocks)
                VisitNodeCollection(paragraph.Inlines);
        }

        private static void Owner_Loaded(object sender, RoutedEventArgs e)
        {
            var owner = (RichTextBlock)sender;
            var status = owner.GetStatus();

            if (status.ForegroundToken == 0)
            {
                status.RequestedThemePropertyToken = owner.RegisterPropertyChangedCallback(FrameworkElement.RequestedThemeProperty, onRequestedThemeChanged);
                status.ForegroundToken = owner.RegisterPropertyChangedCallback(RichTextBlock.ForegroundProperty, onForegroundChanged);
                owner.SizeChanged += onSizeChanged;
            }
        }

        private static void Owner_Unloaded(object sender, RoutedEventArgs e)
        {
            var owner = (RichTextBlock)sender;
            var status = owner.GetStatus();

            if (status.ForegroundToken != 0)
            {
                owner.UnregisterPropertyChangedCallback(FrameworkElement.RequestedThemeProperty, status.RequestedThemePropertyToken);
                owner.UnregisterPropertyChangedCallback(RichTextBlock.ForegroundProperty, status.ForegroundToken);
                owner.SizeChanged -= onSizeChanged;

                status.RequestedThemePropertyToken = 0;
                status.ForegroundToken = 0;
            }
        }

        public static string GetHtml(this FrameworkElement @this)
        {
            return (string)@this.GetValue(HtmlProperty);
        }
        public static void SetHtml(this FrameworkElement @this, string value)
        {
            @this.SetValue(HtmlProperty, value);
        }

        private static Image CreateImage(Element image, Uri baseUri, RichTextBlock owner)
        {
            var src = image.GetAttribute("src");
            if (!Uri.TryCreate(src, UriKind.Absolute, out var uri) &&
                baseUri == null || !Uri.TryCreate(baseUri, src, out uri))
                return null;

            if (!uri.Scheme.StartsWith("http"))
                return null;

            var result = new Image() { Source = UriToBitmapImageConverter.Instance.Convert(uri) };

            result.MaxWidth = owner.ActualWidth;
            result.ImageOpened += Result_ImageOpened;

            if (image.HasAttribute("width"))
                result.Width = double.Parse(image.GetAttribute("width"));
            else
                result.Width = 0;

            if (image.HasAttribute("height"))
                result.Height = double.Parse(image.GetAttribute("height"));
            else
                result.Height = 0;

            return result;
        }

        private static void Result_ImageOpened(object sender, RoutedEventArgs e)
        {
            var owner = (Image)sender;
            if (owner.Width == 0)
            {
                var source = (BitmapImage)owner.Source;
                owner.Width = source.PixelWidth;
                owner.Height = source.PixelHeight;
            }
        }

        private static void CreateElement(InlineCollection parent, Node node, RichTextBlock owner, Uri baseUri)
        {
            switch (node)
            {
                case Element element:
                    switch (element.TagName.ToLower())
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

                                var hyperlink = new Hyperlink() { Foreground = owner.Foreground };
                                hyperlink.NavigateUri = uri;

                                foreach (var child in element.ChildNodes)
                                {
                                    switch (child)
                                    {
                                        case Element childElement:
                                            switch (childElement.TagName.ToLower())
                                            {
                                                case "img":
                                                    if (CreateImage(element, baseUri, owner) is Image image)
                                                    {
                                                        parent.Add(hyperlink);

                                                        parent.Add(new InlineUIContainer()
                                                        {
                                                            Child = new HyperlinkButton()
                                                            {
                                                                NavigateUri = uri,
                                                                Content = image,
                                                                RequestedTheme = owner.RequestedTheme
                                                            }
                                                        });

                                                        hyperlink = new Hyperlink() { Foreground = owner.Foreground };
                                                        hyperlink.NavigateUri = uri;
                                                    }
                                                    break;
                                                default:
                                                    CreateElement(hyperlink.Inlines, child, owner, baseUri);
                                                    break;
                                            }
                                            break;
                                        case Text childText:
                                            CreateElement(hyperlink.Inlines, child, owner, baseUri);
                                            break;
                                    }
                                    break;
                                }

                                if (hyperlink.Inlines.Count != 0)
                                    parent.Add(hyperlink);

                                parent.Add(new Run() { Text = " " });
                            }
                            break;
                        case "img":
                            {
                                if (CreateImage(element, baseUri, owner) is Image image)
                                    parent.Add(new InlineUIContainer() { Child = image });
                            }
                            break;
                        case "strong":
                        case "b":
                            {
                                var span = new Span() { FontWeight = FontWeights.Bold };
                                CreateChildren(span.Inlines, element, owner, baseUri);
                                parent.Add(span);
                            }
                            break;
                        case "div":
                        case "font":
                        case "p":
                        case "span":
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

                                CreateChildren(span.Inlines, element, owner, baseUri);
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
                    break;
                case Text text:
                    parent.Add(new Run() { Text = text.Data });
                    break;
            }
        }

        private static void CreateChildren(InlineCollection parent, Element node, RichTextBlock owner, Uri baseUri)
        {
            foreach (var child in node.ChildNodes)
                CreateElement(parent, child, owner, baseUri);
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
