using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace AWS_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const float marginLeft = 15; //margin nhỏ nhất tính từ viền cửa sổ đến cạnh trái hình ảnh
        const float marginTop = 20; //margin nhỏ nhất tính từ viền cửa sổ đến cạnh trên hình ảnh
        Size maxSize = new Size(640, 360); //kích thước lớn nhất của hình ảnh sau khi nhập vào ứng dụng

        public MainWindow()
        {
            InitializeComponent();
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
            
            if (boundingBoxes.Children.Count != 0)
            {
                boundingBoxes.Children.Clear();
            }

            var originalSize = GetOriginalImageSize(imgPhoto);

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
                Console.WriteLine("Failed to load file " + photo);
                return;
            }

            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();

            DetectLabelsRequest detectlabelsRequest = new DetectLabelsRequest()
            {
                Image = image,
                MaxLabels = 10,
                MinConfidence = 77F
            };

            try
            {
                DetectLabelsResponse detectLabelsResponse = rekognitionClient.DetectLabels(detectlabelsRequest);
                foreach (Amazon.Rekognition.Model.Label label in detectLabelsResponse.Labels)
                {
                    foreach (var instance in label.Instances)
                    {
                        var Left = instance.BoundingBox.Left;
                        var Top = instance.BoundingBox.Top;
                        var Width = instance.BoundingBox.Width;
                        var Height = instance.BoundingBox.Height;
                        var Name = label.Name;

                        DrawBoundingBox(Left, Top, Width, Height, Name, originalSize.Width, originalSize.Height);
                    }
                }
            }
            catch
            {

            }            
        }

        private void DrawBoundingBox(double Left, double Top, double Width, double Height, string Name, double imageWidth, double imageHeight)
        {
            SolidColorBrush colorStroke = new SolidColorBrush(Colors.White);
            SolidColorBrush colorFill = new SolidColorBrush(Colors.Transparent);

            Rectangle boundingBox = new Rectangle();

            boundingBox.Stroke = colorStroke;
            boundingBox.StrokeThickness = 2;
            boundingBox.Fill = colorFill;

            var leftCoordinate = Left * imageWidth + marginLeft; //tọa độ tính từ cạnh trái của hình
            var topCoordinate = Top * imageHeight + marginTop; //tọa độ trên tính từ cạnh trên của hình
            var boundingBoxWidth = Width * imageWidth; //chiều rộng
            var boundingBoxHeight = Height * imageHeight; //chiều dài

            Canvas.SetLeft(boundingBox, leftCoordinate);
            Canvas.SetTop(boundingBox, topCoordinate);

            boundingBox.Width = boundingBoxWidth;
            boundingBox.Height = boundingBoxHeight;

            boundingBox.ToolTip = Name;
            boundingBoxes.Children.Add(boundingBox); 
        }

        private static Size GetOriginalImageSize(System.Windows.Controls.Image image) //hàm lấy kích thước gốc của hình ảnh
        {
            Size result = new Size((int)(image.Source.Width),
                                (int)(image.Source.Height));
            return result;
        }

        public class ListObjects
        {
            public string Name { get; set; }

            public string Confidence { get; set; }
        }
    }
}
