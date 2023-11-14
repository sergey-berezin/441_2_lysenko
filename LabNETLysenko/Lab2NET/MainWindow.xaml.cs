using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

namespace Lab2NET
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
        //public List<ObservableCollection<YoloParser.ImageInfo>> ListImgInfoCollection = new();
        private CancellationTokenSource cts = new();
        List<string> pathList = new();

        private bool cancellationFlag = false;
        private bool clearFlag = false;
        private bool processFlag = false;



        public ICommand ClearOutputFields { get; private set; }
        public ICommand Cancellation { get; private set; }
        public ICommand SelectDirectory { get; private set; }
        public ICommand ProcessPars { get; private set; }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //SelectDirectory = new RelayCommand(_ => { OnSelectDirectory(this); }, CanSelect);
            SelectDirectory = new RelayCommand(_ => { OnSelectDirectory(this); });

            ProcessPars = new RelayCommand(_ => { OnProcessPars(this); }, CanProcess);
            Cancellation = new RelayCommand(_ => { OnCancellation(this); }, CanCancel);
            ClearOutputFields = new RelayCommand(_ => { OnClearFields(this); }, CanClear);

            //SelectDirectory = new AsyncRelayCommand(async _ => { OnSelectDirectory(this); }, CanSelect);
            //ProcessPars = new AsyncRelayCommand(async _ => { OnProcessPars(this); }, CanProcess);
            //Cancellation = new AsyncRelayCommand(async _ => { OnCancellation(this); }, CanCancel);
            //ClearOutputFields = new AsyncRelayCommand(async _ => { OnClearFields(this); }, CanClear);
        }


        private void OnSelectDirectory(object sender)
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
                    pathList.Add(imagePath);
                }
            }
            processFlag = true;
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
                    SaveResults(res.Result);



                if (pathList.Count <= imgInfoCollection.Count)
                {
                    cancellationFlag = false;
                    clearFlag = true;
                    var ordered = imgInfoCollection.OrderBy(info => info.ClassName).ThenByDescending(info => info.Confidence);
                    foreach (var info in ordered)
                    {
                        BitmapSource myImage = ConvertToBitmapSource(info.DetectedObjectImage);
                        CroppedBitmap chunk = new CroppedBitmap(myImage, new Int32Rect((int)info.LeftUpperCornerX, (int)info.LeftUpperCornerY, (int)info.Width, (int)info.Height));
                        //ListParsData.Items.Add($"ClassName= {info.ClassName}; Confidence= {info.Confidence}");
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



        }


        private void ListParsData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var temp = ListParsData.SelectedIndex;
            ImageControl.Source = ConvertToBitmapSource(imgInfoCollection[temp].DetectedObjectImage);
        }

        private BitmapSource ConvertToBitmapSource(Image<Rgb24> image)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Сохранение изображения в формате PNG в поток.
                image.Save(memoryStream, new PngEncoder());

                // Создание нового BitmapSource из потока.
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
            pathList.Clear();
            clearFlag = false;
            processFlag = true;
            //ImageControl.Source = null;
            //ListParsData.Items.Clear();
        }

        private bool CanClear(object sender)
        {
            if (clearFlag)
                return true;
            return false;
        }

        void SaveResults(List<YoloParser.ImageInfo> res)
        {
            foreach (var info in res)
            {
                imgInfoCollection.Add(info);
            }
        }

    }
}
