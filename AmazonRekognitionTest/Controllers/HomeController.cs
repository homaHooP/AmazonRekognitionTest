using Amazon.Rekognition;
using AmazonRekognitionTest.Models;
using AmazonRekognitionTest.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AmazonRekognitionTest.Controllers
{
    public class HomeController(RekognitionService rekognition) : Controller
    {
        private const long MaxImageSize = 5 * 1024 * 1024;

        public IActionResult Index()
        {
            return View(new AnalysisResult());
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile? image, string feature)
        {
            var model = new AnalysisResult { Feature = feature };

            if (image == null || image.Length == 0)
            {
                model.Error = "Select an image.";
                return View(model);
            }
            if (image.Length > MaxImageSize)
            {
                model.Error = "Image must be smaller than 5 MB.";
                return View(model);
            }

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);

            try
            {
                model = await rekognition.AnalyzeAsync(ms, feature);
            }
            catch (AmazonRekognitionException ex)
            {
                model.Error = ex.Message;
            }

            model.ImageDataUrl = $"data:{image.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
