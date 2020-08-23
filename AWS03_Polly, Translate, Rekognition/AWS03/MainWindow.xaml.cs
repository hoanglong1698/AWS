using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using System.IO;
using Amazon.Translate;
using Amazon;
using Amazon.Translate.Model;
using System.Threading;
using Amazon.Polly;
using Amazon.Polly.Model;

namespace AWS03
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VoiceId VoiceName = VoiceId.Joanna;
        public MainWindow()
        {
            InitializeComponent();
        }
        public class Moderation
        {
            public string Name { get; set; }

            public float Confidence { get; set; }

            public string ParentName { get; set; }
        }

        public class Moderation1
        {
            public string Detected { get; set; }

            public float Confidence { get; set; }

            public string Type { get; set; }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
            }

            String photo = imgPhoto.Source.ToString().Substring(8);
            Amazon.Rekognition.Model.Image image = new Amazon.Rekognition.Model.Image();
            try
            {
                using (FileStream fs = new FileStream(photo, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = null;
                    data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);
                    image.Bytes = new MemoryStream(data);
                }
            }
            catch (Exception)
            {
                return;
            }

            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();

            DetectModerationLabelsRequest detectModerationLabelsRequest = new DetectModerationLabelsRequest()
            {
                Image = image,
                MinConfidence = 60F
            };

            try
            {
                DetectModerationLabelsResponse detectModerationLabelsResponse = rekognitionClient.DetectModerationLabels(detectModerationLabelsRequest);
                List<Moderation> items = new List<Moderation>();

                foreach (ModerationLabel label in detectModerationLabelsResponse.ModerationLabels)
                {
                    items.Add(new Moderation() { Name = label.Name, Confidence = label.Confidence, ParentName = label.ParentName });
                    
                }
                lvModeration.ItemsSource = items;
            }
            catch (Exception)
            {
                Console.WriteLine("Error!!!");
                return;
            }

        }

        private void btnLoad1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imgPhoto1.Source = new BitmapImage(new Uri(op.FileName));
            }

            String photo = imgPhoto1.Source.ToString().Substring(8);
            Amazon.Rekognition.Model.Image image = new Amazon.Rekognition.Model.Image();
            try
            {
                using (FileStream fs = new FileStream(photo, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = null;
                    data = new byte[fs.Length];
                    fs.Read(data, 0, (int)fs.Length);
                    image.Bytes = new MemoryStream(data);
                }
            }
            catch (Exception)
            {
                return;
            }

            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();

            DetectTextRequest detectTextRequest = new DetectTextRequest()
            {
                Image = image,
            };

            try
            {
                DetectTextResponse detectTextResponse = rekognitionClient.DetectText(detectTextRequest);

                List<Moderation1> items = new List<Moderation1>();
                foreach (TextDetection text in detectTextResponse.TextDetections)
                {
                    items.Add(new Moderation1() { Confidence = text.Confidence, Detected = text.DetectedText, Type = text.Type });
                }
                lvModeration1.ItemsSource = items;
            }
            catch (Exception)
            {
            }
        }

        private void txtEnglish_TextChanged(object sender, TextChangedEventArgs e)
        {
            var accessKey = "AKIASHKYRPNVFN5WYH4X";
            var secretKey = "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";

            var client = new AmazonTranslateClient(accessKey, secretKey, RegionEndpoint.APSoutheast1);

            var request = new TranslateTextRequest
            {
                Text = txtEnglish.Text,
                SourceLanguageCode = "en",
                TargetLanguageCode = "vi"
            };
            var result = client.TranslateText(request);

            txtVietnamese.Text = result.TranslatedText;
        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            var client = new AmazonPollyClient();
            var request = new SynthesizeSpeechRequest
            {
                Text = txtIn.Text,
                OutputFormat = OutputFormat.Mp3,
                VoiceId = VoiceName
            };

            var response = client.SynthesizeSpeech(request);

            var folder = AppDomain.CurrentDomain.BaseDirectory;

            var filename = $"{folder}{Guid.NewGuid()}.mp3";

            using (var fs = File.Create(filename))
            {
                response.AudioStream.CopyTo(fs);
                fs.Flush();
                fs.Close();
            }

            var player = new MediaPlayer();
            player.Open(new Uri(filename, UriKind.Absolute));

            player.MediaEnded += (s2, e2) => Title = "Demo AWS";
            player.Play();

            Title = "Playing...";

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;

            var name = radioButton.Name;

            if (name == "Joanna")
            {
                VoiceName = VoiceId.Joanna;
            }

            if (name == "Salli")
            {
                VoiceName = VoiceId.Salli;
            }

            if (name == "Matthew")
            {
                VoiceName = VoiceId.Matthew;
            }

            if (name == "Justin")
            {
                VoiceName = VoiceId.Justin;
            }
        }
    }
}
