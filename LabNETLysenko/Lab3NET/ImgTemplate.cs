using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Lab3NET
{
    public class ImgTemplate
    {
        public ImgTemplate(string value, BitmapSource img) { Str = value; Image = img; }
        public string Str { get; set; }
        public BitmapSource Image { get; set; }
    }
    public class ImageInfoForBase64
    {
        public string OriginaImage { get; }
        public int OriginalImageWidth { get; }
        public int OriginalImageHeight { get; }

        public string FileName { get; }
        public int ClassNumber { get; }
        public string ClassName { get; }

        public double Confidence { get; }
        public double LeftUpperCornerX { get; }
        public double LeftUpperCornerY { get; }
        public double Width { get; }
        public double Height { get; }
        public string DetectedObjectImage { get; }
        public int DetectedObjectImageWidth { get; }
        public int DetectedObjectImageHeight { get; }

        public ImageInfoForBase64(string originaImage, int originalImageWidth, int originalImageHeight, string fileName, int classNumber,
            string className, double leftUpperCornerX,
            double leftUpperCornerY, double width, double height,
            string detectedObjectImage, int detectedObjectImageWidth, int detectedObjectImageHeight, double confidence)
        {
            OriginaImage = originaImage;
            OriginalImageWidth = originalImageWidth;
            OriginalImageHeight = originalImageHeight;
            FileName = fileName;
            ClassNumber = classNumber;
            ClassName = className;
            LeftUpperCornerX = leftUpperCornerX;
            LeftUpperCornerY = leftUpperCornerY;
            Width = width;
            Height = height;
            DetectedObjectImage = detectedObjectImage;
            DetectedObjectImageWidth = detectedObjectImageWidth;
            DetectedObjectImageHeight = detectedObjectImageHeight;
            Confidence = confidence;
        }
    }

}
