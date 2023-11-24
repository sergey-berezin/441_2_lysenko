using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YoloParser;
using static Lab4NET.Controllers.ImageAnalysisController;

namespace Lab4NET.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageAnalysisController : ControllerBase
    {
        private readonly ILogger<ImageAnalysisController> logger;

        private Parser pars;

        public ImageAnalysisController(ILogger<ImageAnalysisController> logger, Parser pars)
        {
            this.logger = logger;
            this.pars = pars;
        }

        
        [HttpPost]
        [Route("analyze")]
        public async Task<IActionResult> AnalyzeImage([FromBody] string base64Image)
        {
            try
            {
                

                //var formCollection = await Request.ReadFormAsync();
                //var base64Image = formCollection["image"]; // Retrieve base64 image data

                if (string.IsNullOrEmpty(base64Image))
                {
                    logger.LogInformation("No image data provided. Empty base64");

                    return BadRequest("No image data provided");
                }

                // Convert base64 string to byte array
                byte[] bytes = Convert.FromBase64String(base64Image);
                // Генерация уникального имени файла
                var fileName = Guid.NewGuid().ToString();

                // Сохранение файла на сервере
                var filePath = Path.Combine("C:\\Users\\79250\\Documents\\LabNETLysenko\\Lab4NET\\Data", fileName);
                
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                //Вызов метода для анализа изображения
                var imageInfo = await pars.AnalyzeAsync(filePath, HttpContext.RequestAborted);

                //return Ok(imageInfo);
                // Преобразование результатов в формат JSON
                var jsonResults = imageInfo.Select(result => new
                {
                    result.LeftUpperCornerX,
                    result.LeftUpperCornerY,
                    result.Width,
                    result.Height,
                    result.ClassName,
                    result.Confidence
                });

                logger.LogInformation("Image analysis request processed successfully.");
                return Ok(jsonResults);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (FormatException ex)
            {
                logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during image analysis.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
              
        }

        //private bool IsImageFile(IFormFile file)
        //{
        //    string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
        //    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        //    return !string.IsNullOrEmpty(ext) && permittedExtensions.Contains(ext);
        //}
    }
    
}


