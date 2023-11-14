//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Png;
//using SixLabors.ImageSharp.PixelFormats;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using SixLabors.ImageSharp.Formats.Png;
//using Newtonsoft.Json;

//namespace Lab3NET
//{
//    public class DataStorage
//    {
//        private void SaveToJSON(string filePath)
//        {
//            try
//            {
//                string json = JsonConvert.SerializeObject(imgInfoCollection, Formatting.Indented);
//                File.WriteAllText(filePath, json);
//            }
//            catch (Exception ex)
//            {
//                // Обработка ошибок при сохранении в JSON
//                //MessageBox.Show($"Error saving to JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }

//        private void LoadFromJSON(string filePath)
//        {
//            try
//            {
//                string json = File.ReadAllText(filePath);
//                var loadedData = JsonConvert.DeserializeObject<List<YoloParser.ImageInfo>>(json);

//                imgInfoCollection.Clear();
//                foreach (var item in loadedData)
//                {
//                    imgInfoCollection.Add(item);
//                }
//            }
//            catch (Exception ex)
//            {
//                // Обработка ошибок при загрузке из JSON
//                //MessageBox.Show($"Error loading from JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }



//    }
//}
