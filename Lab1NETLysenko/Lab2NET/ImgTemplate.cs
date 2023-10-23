using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Lab2NET
{
    public class ImgTemplate
    {
        public ImgTemplate(string value, BitmapSource img) { Str = value; Image = img; }
        public string Str { get; set; }
        public BitmapSource Image { get; set; }
    }
}
