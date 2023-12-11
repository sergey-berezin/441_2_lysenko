using Microsoft.AspNetCore.Mvc;
using System;

namespace Lab4NET.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TextProcessingController : Controller
    {
        [HttpPost("processText")]
        public IActionResult ProcessText([FromBody] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BadRequest("No text provided");
            }

            // Преобразование текста в верхний регистр
            var upperCaseText = text.ToUpper();
            return Ok(upperCaseText);
        }
    }
}
