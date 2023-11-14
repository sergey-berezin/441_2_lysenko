using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Net;
using YoloParser;

namespace Lab1NETLysenko
{
    internal class Program
    {
        public class Services : YoloParser.IServices{
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
        private static void SaveResults(List<YoloParser.ImageInfo> res)
        {
            try
            {                
                var csvFileName = DateTime.Now.ToString().Replace(" ", "_").Replace(",", "_").Replace(":", "_") + ".csv";

                foreach (var info in res)
                {
                    info.SaveAsJpeg();
                        
                    if (!File.Exists(csvFileName))
                    {
                        using (var writer = new StreamWriter(csvFileName))
                        {
                            writer.WriteLine($"{nameof(info.FileName)}, " +
                                             $"{nameof(info.ClassNumber)}, " +
                                             $"{nameof(info.ClassName)}, " +
                                             $"{nameof(info.LeftUpperCornerX)}, " +
                                             $"{nameof(info.LeftUpperCornerY)}, " +
                                             $"{nameof(info.Width)}, " +
                                             $"{nameof(info.Height)}");
                        }
                    }

                    using (var writer = new StreamWriter(csvFileName, true))
                    {
                        writer.WriteLine(info.ToString());
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


