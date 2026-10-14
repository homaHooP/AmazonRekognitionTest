namespace AmazonRekognitionTest.Models
{
    public record ResultItem(string Name, float Confidence, string Details = "");

    public record Box(float Left, float Top, float Width, float Height, string Label);

    public class AnalysisResult
    {

        public string Feature { get; set; } = "labels";
        public string? ImageDataUrl { get; set; }
        public string? Error { get; set; }
        public List<ResultItem> Items { get; set; } = [];
        public List<Box> Boxes { get; set; } = [];
    }
}
