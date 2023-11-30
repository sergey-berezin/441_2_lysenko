using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;

namespace TestServer
{
    public class ObjectDetectionControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;
        public ObjectDetectionControllerTest(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }
        [Fact]
        public async Task AnalyzeImage_WithValidBase64_AnswerTest()
        {
            var client = factory.CreateClient();
            string base64Image = Convert.ToBase64String(File.ReadAllBytes("C:\\Users\\79250\\Documents\\LabNETLysenko\\TestServer\\catdog.jpg"));

            var postResponse = await client.PostAsync("https://localhost:7131/ImageAnalysis/analyzeImage", new StringContent(base64Image, Encoding.UTF8, "application/json"));
            var answer = await postResponse.Content.ReadAsStringAsync();
            
            Assert.NotNull(answer);
        }

        [Fact]

        public async Task AnalyzeImage_WithValidBase64_ReturnsOkResult()
        {
            var client = factory.CreateClient();
            string base64Image = Convert.ToBase64String(File.ReadAllBytes("C:\\Users\\79250\\Documents\\LabNETLysenko\\TestServer\\catdog.jpg"));

            var postResponse = await client.PostAsync("https://localhost:7131/ImageAnalysis/analyzeImage", new StringContent(base64Image, Encoding.UTF8, "application/json"));
            var answer = await postResponse.Content.ReadAsStringAsync();

            Assert.Equal(200, (int) postResponse.StatusCode);

        }

        [Fact]
        public async Task AnalyzeImage_WithInvalidBase64_ReturnsBadRequest()
        {
            var client = factory.CreateClient();
            string base64Image = "";

            var postResponse = await client.PostAsync("https://localhost:7131/ImageAnalysis/analyzeImage", new StringContent(base64Image, Encoding.UTF8, "application/json"));
            var answer = await postResponse.Content.ReadAsStringAsync();

            Assert.Equal(400, (int) postResponse.StatusCode);

        }
    }
}