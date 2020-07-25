using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Jarvis
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void pictureButton_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            if (!Plugin.Media.CrossMedia.Current.IsCameraAvailable || !Plugin.Media.CrossMedia.Current.IsTakePhotoSupported)
            {
                return; 
            }
            var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "PictureTest", // 保存先ディレクトリ
                Name = $"{DateTime.UtcNow}.jpg" // 保存ファイル名 
            };
            var file = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
            if (file == null)
                return;
            image.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });

            //試しにローカルのファイルパスを指定してみる
            string filepath = "samplephoto/bill.png";
            ReadText(filepath);
        }

        //Azureサブスクリプション系の設定をここで実施
        static string subscriptionKey = Environment.GetEnvironmentVariable("8ae040a56aad4357adce4b11f1fa4f9a");
        static string endpoint = Environment.GetEnvironmentVariable("https://japaneast.api.cognitive.microsoft.com/");
        static string uriBase = endpoint + "/vision/v3.0//read/analyze";

        //読み込むイメージのパスをここで設定している
        //static string imageFilePath = @"my-image.png";

        //イメージパスから画像を分析するメソッド
        public async void ReadText(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string url = uriBase;
                HttpResponseMessage response;
                string operationLocation;
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(url, content);
                }

                if (response.IsSuccessStatusCode)
                    operationLocation =　response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    string errorString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\n\nResponse:\n{0}\n",JToken.Parse(errorString).ToString());
                    return;
                }


                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1);

                if (i == 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1)
                {
                    Console.WriteLine("\nTimeout error.\n");
                    return;
                }

                Console.WriteLine("\nResponse<ここがCognitive Serviceからのレスポンス！！！！>:\n\n{0}\n",JToken.Parse(contentString).ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        //Image をバイトに変換するメソッド
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
