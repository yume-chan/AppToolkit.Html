using AppToolkit.Converters;
using AppToolkit.Html.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
        private static DependencyProperty UriPorperty { get; } = DependencyProperty.RegisterAttached("Uri", typeof(string), typeof(Html2Xaml), new PropertyMetadata(null));
        private static string GetUri(this DependencyObject @this) => (string)@this.GetValue(UriPorperty);
        private static void SetUri(this DependencyObject @this, string value) => @this.SetValue(UriPorperty, value);

        public static DependencyProperty BaseUriPorperty { get; } = DependencyProperty.RegisterAttached("BaseUri", typeof(Uri), typeof(Html2Xaml), new PropertyMetadata(null, onBaseUriChanged));
        private static void onBaseUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (RichTextBlock)d;
            var status = owner.GetStatus();

            if (status == null)
                return;

            status.BaseUri = (Uri)e.NewValue;

            foreach (var item in status.Images)
            {
                var src = item.GetUri();
                if (!status.TryCreateUri(src, out var uri))
                    continue;

                item.Source = UriToBitmapImageConverter.Instance.Convert(uri);
            }

            foreach (var item in status.Hyperlinks)
            {
                var href = item.GetUri();
                if (!status.TryCreateUri(href, out var uri))
                    continue;

                item.NavigateUri = uri;
            }

            foreach (var item in status.HyperlinkButtons)
            {
                var href = item.GetUri();
                if (!status.TryCreateUri(href, out var uri))
                    continue;

                item.NavigateUri = uri;
            }
        }
        public static Uri GetBaseUri(this FrameworkElement @this) => (Uri)@this.GetValue(BaseUriPorperty);
        public static void SetBaseUri(this FrameworkElement @this, Uri value) => @this.SetValue(BaseUriPorperty, value);

        private class RichTextBlockStatus
        {
            public Uri BaseUri;

            public bool TryCreateUri(string relativeUri, out Uri result) => Uri.TryCreate(relativeUri, UriKind.Absolute, out result) || Uri.TryCreate(BaseUri, relativeUri, out result);

            public Brush Foreground;

            public double ActualWidth;

            public ElementTheme RequestedTheme;

            public long RequestedThemePropertyToken;

            public long ForegroundToken;

            public List<Image> Images { get; } = new List<Image>();

            public List<Hyperlink> Hyperlinks { get; } = new List<Hyperlink>();

            public List<Border> Lines { get; } = new List<Border>();

            public List<HyperlinkButton> HyperlinkButtons { get; } = new List<HyperlinkButton>();
        }
        private static DependencyProperty StatusPorperty { get; } = DependencyProperty.RegisterAttached("Status", typeof(RichTextBlockStatus), typeof(Html2Xaml), new PropertyMetadata(null));
        private static RichTextBlockStatus GetStatus(this FrameworkElement @this) => (RichTextBlockStatus)@this.GetValue(StatusPorperty);
        private static void SetStatus(this FrameworkElement @this, RichTextBlockStatus value) => @this.SetValue(StatusPorperty, value);

        public static DependencyProperty HtmlProperty { get; } = DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Html2Xaml), new PropertyMetadata(null, onHtmlChanged));
        private static void onHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var owner = (RichTextBlock)d;
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

            owner.Blocks.Clear();

            status.BaseUri = owner.GetBaseUri();
            status.Foreground = owner.Foreground;
            status.ActualWidth = owner.ActualWidth;
            status.RequestedTheme = owner.RequestedTheme;

            status.Images.Clear();
            status.Hyperlinks.Clear();
            status.Lines.Clear();
            status.HyperlinkButtons.Clear();

            var paragraph = new Paragraph();
            CreateChildren(paragraph.Inlines, html.Body, status);
            owner.Blocks.Add(paragraph);
        }

        private static void onRequestedThemeChanged(DependencyObject sender, DependencyProperty dp)
        {
            var owner = (RichTextBlock)sender;
            var status = owner.GetStatus();
            var RequestedTheme = owner.RequestedTheme;

            foreach (var item in status.HyperlinkButtons)
                item.RequestedTheme = RequestedTheme;
        }

        private static void onForegroundChanged(DependencyObject sender, DependencyProperty dp)
        {
            var owner = (RichTextBlock)sender;
            var status = owner.GetStatus();
            var Foreground = owner.Foreground;

            foreach (var item in status.Hyperlinks)
                item.Foreground = Foreground;
            foreach (var item in status.Lines)
                item.BorderBrush = Foreground;
        }

        private static void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var owner = (RichTextBlock)sender;
            var status = owner.GetStatus();
            var ActualWidth = e.NewSize.Width;

            foreach (var item in status.Images)
            {
                item.MaxWidth = ActualWidth;

                if (item.Width != 0)
                    item.MaxHeight = ActualWidth / item.Width * item.Height;
            }
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

        private static Image CreateImage(Element image, RichTextBlockStatus status)
        {
            var src = image.GetAttribute("src");
            if (!status.TryCreateUri(src, out var uri))
                return null;

            if (!uri.Scheme.StartsWith("http"))
                return null;

            var result = new Image()
            {
                Source = UriToBitmapImageConverter.Instance.Convert(uri),
                MaxWidth = status.ActualWidth
            };

            result.SetUri(src);
            result.ImageOpened += Result_ImageOpened;

            if (image.HasAttribute("width"))
                result.Width = double.Parse(image.GetAttribute("width"));
            else
                result.Width = 0;

            if (image.HasAttribute("height"))
                result.Height = double.Parse(image.GetAttribute("height"));
            else
                result.Height = 0;

            status.Images.Add(result);
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

            owner.MaxHeight = owner.MaxWidth / owner.Width * owner.Height;
        }

        private static void CreateElement(InlineCollection parent, Node node, RichTextBlockStatus status)
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

                                if (!status.TryCreateUri(href, out var uri))
                                    return;

                                if (!uri.Scheme.StartsWith("http"))
                                    return;

                                parent.Add(new Run() { Text = " " });

                                var hyperlink = new Hyperlink()
                                {
                                    NavigateUri = uri,
                                    Foreground = status.Foreground
                                };

                                foreach (var child in element.ChildNodes)
                                {
                                    switch (child)
                                    {
                                        case Element childElement:
                                            switch (childElement.TagName.ToLower())
                                            {
                                                case "img":
                                                    if (CreateImage(element, status) is Image image)
                                                    {
                                                        if (hyperlink.Inlines.Count != 0)
                                                        {
                                                            hyperlink.SetUri(href);
                                                            status.Hyperlinks.Add(hyperlink);
                                                            parent.Add(hyperlink);
                                                        }

                                                        var button = new HyperlinkButton()
                                                        {
                                                            NavigateUri = uri,
                                                            Content = image,
                                                            RequestedTheme = status.RequestedTheme
                                                        };
                                                        button.SetUri(href);
                                                        status.HyperlinkButtons.Add(button);
                                                        parent.Add(new InlineUIContainer() { Child = button });

                                                        hyperlink = new Hyperlink()
                                                        {
                                                            NavigateUri = uri,
                                                            Foreground = status.Foreground
                                                        };
                                                    }
                                                    break;
                                                default:
                                                    CreateElement(hyperlink.Inlines, child, status);
                                                    break;
                                            }
                                            break;
                                        case Text childText:
                                            CreateElement(hyperlink.Inlines, child, status);
                                            break;
                                    }
                                    break;
                                }

                                if (hyperlink.Inlines.Count != 0)
                                {
                                    hyperlink.SetUri(href);
                                    status.Hyperlinks.Add(hyperlink);
                                    parent.Add(hyperlink);
                                }

                                parent.Add(new Run() { Text = " " });
                            }
                            break;
                        case "img":
                            {
                                if (CreateImage(element, status) is Image image)
                                    parent.Add(new InlineUIContainer() { Child = image });
                            }
                            break;
                        case "strong":
                        case "b":
                            {
                                var span = new Span() { FontWeight = FontWeights.Bold };
                                CreateChildren(span.Inlines, element, status);
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

                                CreateChildren(span.Inlines, element, status);
                                parent.Add(span);
                            }
                            break;
                        case "br":
                            break;
                        case "hr":
                            parent.Add(new LineBreak());

                            var line = new Border()
                            {
                                BorderThickness = new Thickness(0, 1, 0, 0),
                                BorderBrush = status.Foreground,
                                Height = 1,
                                Width = 800
                            };
                            status.Lines.Add(line);
                            parent.Add(new InlineUIContainer() { Child = line });

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

        private static void CreateChildren(InlineCollection parent, Element node, RichTextBlockStatus status)
        {
            foreach (var child in node.ChildNodes)
                CreateElement(parent, child, status);
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
