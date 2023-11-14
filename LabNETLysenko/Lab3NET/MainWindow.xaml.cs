using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab3NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ObservableCollection<YoloParser.ImageInfo> imgInfoCollection = new();
        public ObservableCollection<ImageInfoForBase64> imgInfoCollectionForBase64 = new();

        private CancellationTokenSource cts = new();
        List<string> pathList = new();

        private bool cancellationFlag = false;
        private bool clearFlag = false;
        private bool processFlag = false;


        public ICommand ClearJsonFiles { get; private set; }
        public ICommand ClearOutputFields { get; private set; }
        public ICommand Cancellation { get; private set; }
        public ICommand SelectDirectory { get; private set; }
        public ICommand ProcessPars { get; private set; }


        private void LoadAndDisplayData()
        {
            LoadFromJSON("data.json", imgInfoCollection);

            if (imgInfoCollection.Count > 0)
            {
                var ordered = imgInfoCollection.OrderBy(info => info.ClassName).ThenByDescending(info => info.Confidence);
                foreach (var info in ordered)
                {
                    BitmapSource myImage = ConvertToBitmapSource(info.DetectedObjectImage); 
                    CroppedBitmap chunk = new CroppedBitmap(myImage, new Int32Rect((int)info.LeftUpperCornerX, (int)info.LeftUpperCornerY, (int)info.Width, (int)info.Height));
                    ListParsData.Items.Add(new ImgTemplate($"ClassName= {info.ClassName}; Confidence= {info.Confidence}", chunk));
                }
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            LoadAndDisplayData();
            DataContext = this;
            SelectDirectory = new RelayCommand(_ => { OnSelectDirectory(this); });
            ClearJsonFiles = new RelayCommand(_ => { OnClearJsonFiles(this); });
            ProcessPars = new RelayCommand(_ => { OnProcessPars(this); }, CanProcess);
            Cancellation = new RelayCommand(_ => { OnCancellation(this); }, CanCancel);
            ClearOutputFields = new RelayCommand(_ => { OnClearFields(this); }, CanClear);
        }

        private void OnSelectDirectory(object sender)
        {
            try
            {
                var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

                if (dlg.ShowDialog(this).GetValueOrDefault())
                {
                    pathList.Clear();
                    foreach (var imagePath in Directory.GetFiles(dlg.SelectedPath)
                        .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!imgInfoCollectionForBase64.Any(item => AreImagesEqual(Convert.FromBase64String(item.OriginaImage), ConvertImageToByte(SixLabors.ImageSharp.Image.Load<Rgb24>(imagePath)))))
                        {
                            pathList.Add(imagePath);
                        }
                    }
                }
                processFlag = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private bool AreImagesEqual(byte[] bytes1, byte[] bytes2)
        {

            if (bytes1.Length != bytes2.Length)
            {
                return false;
            }

            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                {
                    return false;
                }
            }

            return true;
        }
        public class Services : YoloParser.IServices
        {
            public bool Exists(string path) => File.Exists(path);
            public void WriteBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);
            public void Print(string msg) => Console.WriteLine(msg);

        }


        private async void OnProcessPars(object sender)
        {
            CancellationToken ctn = cts.Token;
            try
            {
                cancellationFlag = true;
                processFlag = false;

                var pars = new YoloParser.Parser(new Services());

                var tasks = Enumerable.Range(0, pathList.Count).Select(i =>
                {
                    return pars.AnalyzeAsync(pathList[i], ctn);
                }).ToArray();

                await Task.WhenAll(tasks);
                foreach (var res in tasks)
                {
                    SaveResults(res.Result);
                }


                if (pathList.Count <= imgInfoCollection.Count)
                {
                    //SaveToJSON("data.json", imgInfoCollection);

                    cancellationFlag = false;
                    clearFlag = true;
                    ImageControl.Source = null;
                    ListParsData.Items.Clear();
                    var ordered = imgInfoCollection.OrderBy(info => info.ClassName).ThenByDescending(info => info.Confidence);
                    foreach (var info in ordered)
                    {
                        BitmapSource myImage = ConvertToBitmapSource(info.DetectedObjectImage);
                        CroppedBitmap chunk = new CroppedBitmap(myImage, new Int32Rect((int)info.LeftUpperCornerX, (int)info.LeftUpperCornerY, (int)info.Width, (int)info.Height));
                        ListParsData.Items.Add(new ImgTemplate($"ClassName= {info.ClassName}; Confidence= {info.Confidence}", chunk));
                    }

                }
            }
            catch (OperationCanceledException)
            {
                cts = new CancellationTokenSource();
                cancellationFlag = false;
                processFlag = false;
                clearFlag = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            finally
            {
                SaveToJSON("data.json", imgInfoCollection);
            }
        }


        private void ListParsData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListParsData.SelectedIndex >= 0 && ListParsData.SelectedIndex < imgInfoCollection.Count)
            {
                var temp = ListParsData.SelectedIndex;
                ImageControl.Source = ConvertToBitmapSource(imgInfoCollection[temp].DetectedObjectImage);
            }
        }

        private BitmapSource ConvertToBitmapSource(Image<Rgb24> image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, new PngEncoder());
                var bitmapSource = new BitmapImage();
                bitmapSource.BeginInit();
                bitmapSource.StreamSource = new MemoryStream(memoryStream.ToArray());
                bitmapSource.CacheOption = BitmapCacheOption.OnLoad;

                bitmapSource.EndInit();
                bitmapSource.StreamSource.Dispose();

                return bitmapSource;
            }
        }

        //private bool CanSelect(object sender)
        //{
        //    if (!processFlag)
        //        return true;
        //    return false;
        //}

        private bool CanProcess(object sender)
        {
            if (processFlag)
                return true;
            return false;
        }

        private void OnCancellation(object sender)
        {
            cts.Cancel();
            cancellationFlag = false;
        }
        private bool CanCancel(object sender)
        {
            if (cancellationFlag)
                return true;
            return false;
        }

        private void OnClearFields(object sender)
        {
            processFlag = true;

            SaveToJSON("data.json", imgInfoCollection);
            imgInfoCollection.Clear();
            pathList.Clear();
            //clearFlag = false;
            //processFlag = true;
            ListParsData.Items.Clear();
            ImageControl.Source = null;
            clearFlag = false;

        }

        private bool CanClear(object sender)
        {
            if (clearFlag)
                return true;
            return false;
        }

        private void OnClearJsonFiles(object sender)
        {
            string jsonFilePath = "data.json";
            string backupPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(jsonFilePath), System.IO.Path.GetFileNameWithoutExtension(jsonFilePath) + "_backup.json");

            if (File.Exists(jsonFilePath))
            {
                File.Delete(jsonFilePath);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }


        void SaveResults(List<YoloParser.ImageInfo> res)
        {
            foreach (var info in res)
            {
                imgInfoCollection.Add(info);
            }
        }

        private Image<Rgb24> ConvertBase64ToImage(string base64, int width, int height)
        {
            byte[] rgbBytes = Convert.FromBase64String(base64);
            return SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(rgbBytes, width, height);
            //    byte[] imageBytes = Convert.FromBase64String(base64);
            //using (MemoryStream ms = new MemoryStream(imageBytes))
            //{
            //    return SixLabors.ImageSharp.Image.Load<Rgb24>(ms);
            //}
        }
        private byte[] ConvertImageToByte(Image<Rgb24> image)
        {
            byte[] pixelBytes = new byte[image.Width * image.Height * Unsafe.SizeOf<Rgb24>()];

            image.CopyPixelDataTo(pixelBytes);

            return pixelBytes;
            //var MemoryGroup = image.GetPixelMemoryGroup();
            //var Array = MemoryGroup.ToArray()[0];
            //return Convert.ToBase64String(MemoryMarshal.AsBytes(Array.Span));
            //using (MemoryStream ms = new())
            //{
            //    //ms.Flush();
            //    //ms.Seek(0, SeekOrigin.Begin);
            //    image.SaveAsJpeg(ms);
            //    byte[] imageBytes = ms.ToArray();

            //    return imageBytes;
            //    //return Convert.ToBase64String(imageBytes);
            //}
        }


        private void SaveToJSON(string filePath, ObservableCollection<YoloParser.ImageInfo> imgInfoCollection)
        {
            try 
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }
                //string backupPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath) + "_backup.json");
                //File.Copy(filePath, backupPath, true);

                if (imgInfoCollection != null)
                {
                    foreach (var info in imgInfoCollection)
                    {

                        byte[] OriginaImage = ConvertImageToByte(info.OriginaImage);
                        byte[] DetectedObjectImage = ConvertImageToByte(info.DetectedObjectImage);
                        int OriginaImageWidth = info.OriginaImage.Width;
                        int OriginalImageHeight = info.OriginaImage.Height;
                        int DetectedObjectImageWidth = info.DetectedObjectImage.Width;
                        int DetectedObjectImageHeight = info.DetectedObjectImage.Height;
                        if (!imgInfoCollectionForBase64.Any(item => AreImagesEqual(Convert.FromBase64String(item.OriginaImage), OriginaImage)))
                        {
                            imgInfoCollectionForBase64.Add(
                            new ImageInfoForBase64(
                                Convert.ToBase64String(OriginaImage),
                                OriginaImageWidth,
                                OriginalImageHeight,
                                info.FileName,
                                info.ClassNumber,
                                info.ClassName,
                                info.LeftUpperCornerX,
                                info.LeftUpperCornerY,
                                info.Width,
                                info.Height,
                                Convert.ToBase64String(DetectedObjectImage),
                                DetectedObjectImageWidth,
                                DetectedObjectImageHeight,
                                info.Confidence
                                )
                            );
                        }

                            //imgInfoCollectionForBase64.Add(
                            //new ImageInfoForBase64(
                            //    Convert.ToBase64String(OriginaImage),
                            //    info.FileName,
                            //    info.ClassNumber,
                            //    info.ClassName,
                            //    info.LeftUpperCornerX,
                            //    info.LeftUpperCornerY,
                            //    info.Width,
                            //    info.Height,
                            //    Convert.ToBase64String(DetectedObjectImage),
                            //    info.Confidence
                            //    )
                            //);

                    }
                }

                string json = JsonConvert.SerializeObject(imgInfoCollectionForBase64, Formatting.Indented);
                File.WriteAllText(filePath, json);

                string backupPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath) + "_backup.json");
                File.Copy(filePath, backupPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFromJSON(string filePath, ObservableCollection<YoloParser.ImageInfo> imgInfoCollection)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }
                string backupPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath) + "_backup.json");
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, filePath, true);
                    File.Delete(backupPath);
                }

                string json = File.ReadAllText(filePath);
                var loadedData = JsonConvert.DeserializeObject<ObservableCollection<ImageInfoForBase64>>(json);

                imgInfoCollection.Clear();
                imgInfoCollectionForBase64.Clear();
                if (loadedData != null)
                {
                    imgInfoCollectionForBase64 = loadedData;
                    foreach (var item in loadedData)
                    {
                        Image<Rgb24> OriginaImage = ConvertBase64ToImage(item.OriginaImage, item.OriginalImageWidth , item.OriginalImageHeight);
                        Image<Rgb24> DetectedObjectImage = ConvertBase64ToImage(item.DetectedObjectImage, item.DetectedObjectImageWidth, item.DetectedObjectImageHeight);
                        imgInfoCollection.Add(
                            new YoloParser.ImageInfo(
                                OriginaImage,
                                item.FileName,
                                item.ClassNumber,
                                item.ClassName,
                                item.LeftUpperCornerX,
                                item.LeftUpperCornerY,
                                item.Width,
                                item.Height,
                                DetectedObjectImage,
                                item.Confidence)
                        );
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading from JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
