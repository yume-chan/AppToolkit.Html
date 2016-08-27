using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace AppToolkit.Converters
{
    internal sealed class UriToBitmapImageConverter : IValueConverter
    {
        public static UriToBitmapImageConverter Instance { get; } = new UriToBitmapImageConverter();

        static readonly ThreadLocal<Dictionary<Uri, WeakReference<BitmapImage>>> store = new ThreadLocal<Dictionary<Uri, WeakReference<BitmapImage>>>(() => new Dictionary<Uri, WeakReference<BitmapImage>>());

        public BitmapImage Convert(Uri source)
        {
            if (source == null)
                return null;

            var dict = store.Value;

            WeakReference<BitmapImage> wr;
            BitmapImage bi;
            if (dict.TryGetValue(source, out wr) && wr.TryGetTarget(out bi))
                return bi;
            else
            {
                bi = new BitmapImage();
                SetUriSourceAsync(bi, source);
                wr = new WeakReference<BitmapImage>(bi);
                dict[source] = wr;
                return bi;
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Uri uri)
                return Convert(uri);
            else if (value is string s)
                return Convert(new Uri(s, UriKind.Absolute));
            else
                return (Uri)value; // throw InvalidCastException;
        }

        static readonly HttpClient httpClient = new HttpClient();

        static async void SetUriSourceAsync(BitmapSource image, Uri uri)
        {
            try
            {
                using (var inputStream = await httpClient.GetInputStreamAsync(uri))
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await RandomAccessStream.CopyAsync(inputStream, stream);
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                }
            }
            catch
            {
                store.Value.Remove(uri);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
