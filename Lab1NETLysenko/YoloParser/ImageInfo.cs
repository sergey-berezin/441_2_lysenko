﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloParser
{
    public class ImageInfo
    {
        public string FileName { get; }
        public int ClassNumber { get; }
        public string ClassName { get; }

        public double Confidence { get; }
        public double LeftUpperCornerX { get; }
        public double LeftUpperCornerY { get; }
        public double Width { get; }
        public double Height { get; }
        public Image<Rgb24> DetectedObjectImage { get; }

        public ImageInfo(string fileName, int classNumber,
            string className, double leftUpperCornerX,
            double leftUpperCornerY, double width, double height,
            Image<Rgb24> detectedObjectImage, double confidence)
        {
            FileName = fileName;
            ClassNumber = classNumber;
            ClassName = className;
            LeftUpperCornerX = leftUpperCornerX;
            LeftUpperCornerY = leftUpperCornerY;
            Width = width;
            Height = height;
            DetectedObjectImage = detectedObjectImage;
            Confidence = confidence;
        }

        public void SaveAsJpeg()
        {
            DetectedObjectImage.SaveAsJpeg(FileName);
        }

        public override string ToString()
        {
            return FileName + ", " + ClassNumber.ToString() + ", " + ClassName +
                ", " + LeftUpperCornerX.ToString().Replace(',', '.') + ", " + LeftUpperCornerY.ToString().Replace(',', '.') +
                ", " + Width.ToString().Replace(',', '.') + ", " + Height.ToString().Replace(',', '.');
        }
    }
}
