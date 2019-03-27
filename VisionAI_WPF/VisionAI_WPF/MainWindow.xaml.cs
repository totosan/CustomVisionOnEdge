using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VisionAI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
	{
        HttpClient _client = new HttpClient();

        public MainWindow()
		{
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
		}

	

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += GetFrameAsync;
            timer.Start();

            _client.BaseAddress = new Uri("http://127.0.0.1:8081/image");

            VideoPlayer.Source = new Uri(@"C:\Users\toto_\OneDrive\Bilder\alerts2\video\RQGBG\1_2018-09-23_07-58-23.mp4", UriKind.Absolute);
			VideoPlayer.LoadedBehavior = MediaState.Manual;
			VideoPlayer.Play();

		}

        private async Task<string> UploadImage(RenderTargetBitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(stream);

            MultipartFormDataContent form = new MultipartFormDataContent();
            HttpContent content = new StringContent("fileToUpload");
            form.Add(content, "fileToUpload");
            
            content = new StreamContent(stream);
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "fileToUpload",
                FileName = "whatever"
            };
            form.Add(content);
            var response = await _client.PostAsync("", form);
            return response.Content.ReadAsStringAsync().Result;
        }

        async void GetFrameAsync(object sender, EventArgs e)
		{
			Size dpi = new Size(96, 96);
			RenderTargetBitmap bmp =
			  new RenderTargetBitmap(300, 200,
				dpi.Width, dpi.Height, PixelFormats.Pbgra32);
			bmp.Render(VideoPlayer);

            var result = await UploadImage(bmp);

            var rect = new Rectangle();
			rect.Stroke = new SolidColorBrush(Color.FromRgb(100,0,0));
			rect.StrokeThickness = 5;
			rect.Height = 100;
			rect.Width = 100;
			rect.Margin = new Thickness(100,10,0,0);
			painting.Children.Add(rect);
		}
	}
} 
