using Microsoft.ProjectOxford.Emotion;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rover;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmoRecognizer
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static string OxfordAPIKey = "2cabd9f1b2014a04bc04782b3c703539";

        MediaCapture MC;
        DispatcherTimer RecognitionTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(5) };
        EmotionServiceClient Oxford = new EmotionServiceClient(OxfordAPIKey);

        EmoCollection MyEmo = new EmoCollection();

        TwoMotorsDriver Rover = new TwoMotorsDriver(new Motor(27, 22), new Motor(5, 6));

        CloudBlobContainer ImagesDir;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Init();

            RecognitionTimer.Tick += GetEmotions;
            RecognitionTimer.Start();
        }

        private async Task<CloudBlobContainer> GetImagesBlobContainer()
        {

            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=hackstore;AccountKey=lSUhDclnBi8C37C0Z73C9xZatVgGb1U+tkrPGnz67UJLB8ZSiGNVNejvdMoSf6j4uhp/VKysfdK8U2BDaJaLYw==");
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("surprisefaces");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            return container;
        }

        private async Task<string> SendPicture(MemoryStream ms)
        {
            var name = Guid.NewGuid().ToString() + ".jpg";
            var b = ImagesDir.GetBlockBlobReference(name);
            b.Properties.ContentType = "image/jpeg";
            await b.UploadFromStreamAsync(ms.AsInputStream());
            return $"http://hackstore.blob.core.windows.net/surprisefaces/{name}";
        }

        private async Task Init()
        {
            ImagesDir = await GetImagesBlobContainer();
            MC = new MediaCapture();
            var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var camera = cameras.First();
            var settings = new MediaCaptureInitializationSettings() { VideoDeviceId = camera.Id };
            settings.StreamingCaptureMode = StreamingCaptureMode.Video;
            await MC.InitializeAsync(settings);
            ViewFinder.Source = MC;
            await MC.StartPreviewAsync();

            FaceTrackerProxy proxyDetector = new FaceTrackerProxy(VisCanvas, this, ViewFinder, MC);
            proxyDetector.OnFaceDetected += ProxyDetector_OnFaceDetected;
        }

        DateTime RecoBlock = DateTime.Now;

        private async void ProxyDetector_OnFaceDetected(object sender, FaceDetectionEventArgs ea)
        {
            // Debug.WriteLine(ea.X);
            if (DateTime.Now>RecoBlock)
            {
                RecoBlock = DateTime.Now.AddSeconds(5);
                var x = ea.X;
                if (x < 200) await Rover.TurnRightAsync(150);
                if (x > 400) await Rover.TurnLeftAsync(150);
            }
        }

        bool SentSurprize = false;

        async void GetEmotions(object sender, object e)
        {
            // dt.Stop();
            var ms = new MemoryStream();

            //FAKE IMAGEjjk
            //BitmapImage bitmapImage = new BitmapImage();
            ////img.Width = bitmapImage.DecodePixelWidth = 280;
            Uri uri = new Uri("ms-appx:///Assets/WIN_20160205_23_45_55_Pro.jpg");
            //bitmapImage.UriSource = uri;
            //img.Source = bitmapImage;

            //StorageFile storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);

            //var randomAccessStream = await storageFile.OpenReadAsync();
            //Stream stream = randomAccessStream.AsStreamForRead();

            //await stream.CopyToAsync(ms);
            ////END OF FAKE IMAGE

            //Stream stream = randomAccessStream.AsStreamForRead(); 
            try
            {
                int i = 5;
                int j = i / 0;
            }
            catch
            {
            }

            try
            {
                throw new Exception("demo");
            }
            catch
            {

            }
            
            await MC.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), ms.AsRandomAccessStream());

            ms.Position = 0L;
            var ms1 = new MemoryStream();
            await ms.CopyToAsync(ms1);
            ms.Position = 0L;
            var Emo = await Oxford.RecognizeAsync(ms);
            if (Emo!=null && Emo.Length>0)
            {
                var Face = Emo[0];
                var s = Face.Scores;

                if (s.Surprise > 0.8)
                {
                    if (!SentSurprize)
                    {
                        ms1.Position = 0L;
                        var u = await SendPicture(ms1);
                        await RoverServices.InsertSF(u, s.Surprise);
                        SentSurprize = true;
                    }
                }
                else SentSurprize = false;
                // res.Text = $"Happiness: {s.Happiness,6:N4}\nAnger: {s.Anger,6:N4}\nContempt: {s.Contempt,6:N4}\nDisgust: {s.Disgust,6:N4}\nFear: {s.Fear,6:N4}\nSadness: {s.Sadness,6:N4}\nSurprise: {s.Surprise,6:N4}";
                // Canvas.SetLeft(res, Face.FaceRectangle.Left+Face.FaceRectangle.Width/2);
                // Canvas.SetTop(res, Face.FaceRectangle.Top+Face.FaceRectangle.Height/2);
                var T = new Thickness();
                T.Left = Face.FaceRectangle.Left;
                T.Top=Face.FaceRectangle.Top;
                // res.Margin = T;
                // EmoControl.Margin = T;
                MyEmo.Update(Face.Scores);
                await RoverServices.Insert(Face.Scores);
            }
        }


    }

}
