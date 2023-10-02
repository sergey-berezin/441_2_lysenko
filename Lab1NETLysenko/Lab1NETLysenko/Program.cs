using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Net;
using YoloParser;

namespace Lab1NETLysenko
{
    internal class Program
    {
        public class Services : YoloParser.IServices
        {
            public bool Exists(string path) => File.Exists(path);
            public void WriteBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);
            public void Print(string msg) => Console.WriteLine(msg);

        }
        static async Task Main(string[] args)
        {
            var pars = new YoloParser.Parser(new Services());
            var tokenSource = new CancellationTokenSource();
            var tasks = Enumerable.Range(0, args.Length).Select(i =>
            {
                var t = pars.AnalyzeAsync(args[i], tokenSource.Token);
                Console.WriteLine($"{i}");
                return t;
            }).ToArray();

            Console.WriteLine("Enter = continue, 'z' = cancel detection");
            if (Console.ReadLine() == "z")
                tokenSource.Cancel();

            try
            {
                await Task.WhenAny(tasks);
                foreach (var res in tasks)
                    SaveResults(res.Result);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Console.WriteLine("Detecting cancelled!");
            }
        }
        private static void SaveResults((string fileName, List<ObjectBox> objects) res)
        {
            try
            {
                string filePath = "Result\\table.csv";
                string[] labels = new string[]
                {
                "aeroplane", "bicycle", "bird", "boat", "bottle",
                "bus", "car", "cat", "chair", "cow",
                "diningtable", "dog", "horse", "motorbike", "person",
                "pottedplant", "sheep", "sofa", "train", "tvmonitor"
                };
                if (!File.Exists(filePath))
                {
                    using FileStream fs = File.Create(filePath);
                }
                using (StreamWriter writer = new(filePath, true))
                {
                    if (new FileInfo(filePath).Length == 0)
                    {
                        writer.WriteLine("filename,class,x,y,w,h");
                    }
                    foreach (var obj in res.objects)
                    {
                        writer.WriteLine($"{res.fileName},{labels[obj.Class]},{obj.XMin},{obj.YMax},{obj.XMax - obj.XMin},{obj.YMax - obj.YMin}");
                    }
                }
            }
            catch (Exception x)
            {
                Console.WriteLine($"Error saving file!: {x}");
            }
        }
    }
}


