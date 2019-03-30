using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VisionAI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient _client = new HttpClient();
        private Uri _uri;
        private int _vidHeight;
        private int _vidWidth;
        private Size _dpi;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.6d);
            timer.Tick += GetFrameAsync;
            timer.Start();

            var path = ConfigurationManager.AppSettings["path"];
            var uri = ConfigurationManager.AppSettings["uri"];
            _uri = new Uri(uri);


            VideoPlayer.Source = new Uri(path, UriKind.Absolute);
            VideoPlayer.LoadedBehavior = MediaState.Manual;
            VideoPlayer.MediaOpened += (o, args) =>
            {
                _vidHeight = VideoPlayer.NaturalVideoHeight;
                _vidWidth = VideoPlayer.NaturalVideoWidth;
                _dpi = new Size(96, 96);

            };
            VideoPlayer.Play();


        }

        private async Task<string> UploadImage(RenderTargetBitmap bmp)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);

                var byteData = stream.ToArray();
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    var response = await _client.PostAsync(_uri, content);
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
        }

        async void GetFrameAsync(object sender, EventArgs e)
        {
            if (!VideoPlayer.HasVideo)
                return;

            RenderTargetBitmap bmp =
              new RenderTargetBitmap(_vidWidth, _vidHeight,
                _dpi.Width, _dpi.Height, PixelFormats.Pbgra32);
            bmp.Render(VideoPlayer);

            painting.Children?.Clear();

            var result = await UploadImage(bmp);
            var predictions = PredictionResult.FromJson(result);
            var relevant = predictions.Predictions.Where(x => x.Probability > 0.5);

            int i = 0;
            foreach (var prediction in relevant)
            {
                var boundingBox = new Rect(
                    prediction.BoundingBox.Left,
                    prediction.BoundingBox.Top,
                    prediction.BoundingBox.Width,
                    prediction.BoundingBox.Height);

                var rect = new Rectangle();
                rect.Stroke = new SolidColorBrush(Color.FromRgb(100, (byte)(i * 10), 0));
                rect.StrokeThickness = 1;
                rect.Height = boundingBox.Height * _vidHeight;
                rect.Width = boundingBox.Width * _vidWidth;
                rect.Margin = new Thickness(boundingBox.Left * _vidWidth, boundingBox.Top * _vidHeight, 0, 0);

                var text = new TextBlock();
                text.Text = prediction.TagName;
                text.Margin = rect.Margin;
                text.Background = new SolidColorBrush(Color.FromRgb(100, (byte)(i * 10), 0));
                text.Foreground = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                painting.Children.Add(text);
                painting.Children.Add(rect);
            }
        }
    }
}
