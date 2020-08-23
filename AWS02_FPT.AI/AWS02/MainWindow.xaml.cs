using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace AWS02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        class TextToSpeechData
        {
            public string async { get; set; }
        }

        class SpeechToTextData
        {
            public List<DictionaryEntry> hypotheses { get; set; }
        }

        class DictionaryEntry
        {
            public string utterance { get; set; }
        }

        public string voice = "banmai";
        public DispatcherTimer timer;
        public double duration = 0;
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;
        public string fileNameRecord;
       
        private void btnTextToSpeech_Click(object sender, RoutedEventArgs e)
        {
            string payload = InputText.Text;

            if (payload == "")
            {
                MessageBox.Show("Please type something");
                return;
            }
            else
            {
                Title = "Processing...";
                String json = Task.Run(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("api-key", "3VCb8YQaGQOTqvNwGtf5GCg1ZZM1AyB6");
                    client.DefaultRequestHeaders.Add("speed", "");
                    client.DefaultRequestHeaders.Add("voice", voice);
                    var response = await client.PostAsync("https://api.fpt.ai/hmi/tts/v5", new StringContent(payload));
                    return await response.Content.ReadAsStringAsync();
                }).GetAwaiter().GetResult();

                var data = JsonConvert.DeserializeObject<TextToSpeechData>(json);
                var folder = AppDomain.CurrentDomain.BaseDirectory;
                var filename = $"{folder}{Guid.NewGuid()}.mp3";

                using (var client = new WebClient())
                {
                    Title = "Downloading...";
                    client.DownloadFile(data.async, filename);
                }

                Title = "Downloaded";

                var player = new MediaPlayer();
                player.Open(new Uri(filename, UriKind.Absolute));
                player.Play();
                player.MediaEnded += (s2, e2) => Title = "Player ended";

                Title = "Playing audio";
            }
        }

        private void btnUploadAudioFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a voice clip";
            op.Filter = "All Supported Audio | *.mp3; *.wma | MP3s | *.mp3 | WMAs | *.wma";
            if (op.ShowDialog() == true)
            {
                Title = "Processing...";
                var filename = op.FileName;
                var payload = File.ReadAllBytes(filename);

                String json = Task.Run(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("api-key", "3VCb8YQaGQOTqvNwGtf5GCg1ZZM1AyB6");
                    var response = await client.PostAsync("https://api.fpt.ai/hmi/asr/general", new ByteArrayContent(payload));
                    return await response.Content.ReadAsStringAsync();
                }).GetAwaiter().GetResult();

                var data = JsonConvert.DeserializeObject<SpeechToTextData>(json);
                OutputText.Text = data.hypotheses[0].utterance;

                Title = "Done";
            }
            else
            {
                MessageBox.Show("Can't open file, please try again");
                return;
            }
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnRecord.IsEnabled = false;
                btnRecord.Content = "Recording...";
                duration = 0;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += timer_Tick;
                timer.Start();

                waveSource = new WaveIn();
                waveSource.WaveFormat = new WaveFormat(44100, 1);

                waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
                waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

                var folder = AppDomain.CurrentDomain.BaseDirectory;
                fileNameRecord = $"{folder}{Guid.NewGuid()}.wav";
                waveFile = new WaveFileWriter(fileNameRecord, waveSource.WaveFormat);

                waveSource.StartRecording();
            }
            catch (Exception)
            {

                throw;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            duration++;
            durationTextBlock.Text = TimeSpan.FromSeconds(duration).ToString();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            waveSource.StopRecording();
            timer.Stop();
            btnStop.IsEnabled = false;
            btnStop.Content = "Processing...";
            btnRecord.IsEnabled = true;
            btnRecord.Content = "Record";
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

            var payload = File.ReadAllBytes(fileNameRecord);

            String json = Task.Run(async () =>
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("api-key", "3VCb8YQaGQOTqvNwGtf5GCg1ZZM1AyB6");
                var response = await client.PostAsync("https://api.fpt.ai/hmi/asr/general", new ByteArrayContent(payload));
                return await response.Content.ReadAsStringAsync();
            }).GetAwaiter().GetResult();

            var data = JsonConvert.DeserializeObject<SpeechToTextData>(json);
            OutputText.Text = data.hypotheses[0].utterance;

            btnStop.IsEnabled = true;
            btnStop.Content = "Stop";
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;

            voice = radioButton.Name;
        }
    }
}
