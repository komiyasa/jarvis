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
using Jarvis.Services;
using Plugin.Media.Abstractions;

namespace Jarvis
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        IFaceRecognitionService _faceRecognitionService;
        MediaFile photo;

        public MainPage()
        {
            InitializeComponent();
            _faceRecognitionService = new FaceRecognitionService();
        }

        private async void pictureButton_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            // Take photo
            if (CrossMedia.Current.IsCameraAvailable || CrossMedia.Current.IsTakePhotoSupported)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Name = "emotion.jpg",
                    PhotoSize = PhotoSize.Small
                });

                if (photo != null)
                {
                    image.Source = ImageSource.FromStream(photo.GetStream);
                }
            }
            else
            {
                await DisplayAlert("No Camera", "Camera unavailable.", "OK");
            }

            ((Button)sender).IsEnabled = false;
            activityIndicator.IsRunning = true;

            // Recognize emotion
            try
            {
                if (photo != null)
                {
                    var faceAttributes = new FaceAttributeType[] { FaceAttributeType.Emotion };
                    using (var photoStream = photo.GetStream())
                    {
                        Face[] faces = await _faceRecognitionService.DetectAsync(photoStream, true, false, faceAttributes);
                        if (faces.Any())
                        {
                            // Emotions detected are happiness, sadness, surprise, anger, fear, contempt, disgust, or neutral.
                            emotionResultLabel.Text = faces.FirstOrDefault().FaceAttributes.Emotion.ToRankedList().FirstOrDefault().Key;
                        }
                        photo.Dispose();
                    }
                }
            }
            catch (FaceAPIException fx)
            {
                Debug.WriteLine(fx.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            activityIndicator.IsRunning = false;
            ((Button)sender).IsEnabled = true;

        }
    }
}
