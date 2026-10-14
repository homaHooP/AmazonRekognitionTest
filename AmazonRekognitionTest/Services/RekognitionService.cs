using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using AmazonRekognitionTest.Models;

namespace AmazonRekognitionTest.Services
{
    public class RekognitionService(IAmazonRekognition client)
    {
        public async Task<AnalysisResult> AnalyzeAsync(MemoryStream image, string feature)
        {
            var result = new AnalysisResult { Feature = feature };
            var img = new Image { Bytes = image };

            switch (feature)
            {
                case "labels":
                    var labels = await client.DetectLabelsAsync(new DetectLabelsRequest { Image = img, MaxLabels = 20, MinConfidence = 70 });
                    foreach (var l in labels.Labels ?? [])
                    {
                        var parents = string.Join(", ", (l.Parents ?? []).Select(p => p.Name));
                        result.Items.Add(new(l.Name, l.Confidence ?? 0, parents));
                        foreach (var i in l.Instances ?? [])
                            result.Boxes.Add(ToBox(i.BoundingBox, l.Name));
                    }
                    break;

                case "moderation":
                    var moderation = await client.DetectModerationLabelsAsync(new DetectModerationLabelsRequest { Image = img, MinConfidence = 50 });
                    foreach (var m in moderation.ModerationLabels ?? [])
                        result.Items.Add(new(m.Name, m.Confidence ?? 0, m.ParentName));
                    break;

                case "text":
                    var text = await client.DetectTextAsync(new DetectTextRequest { Image = img });
                    foreach (var t in (text.TextDetections ?? []).Where(t => t.Type == TextTypes.LINE))
                    {
                        result.Items.Add(new(t.DetectedText, t.Confidence ?? 0));
                        result.Boxes.Add(ToBox(t.Geometry?.BoundingBox, t.DetectedText));
                    }
                    break;

                case "faces":
                    var faces = await client.DetectFacesAsync(new DetectFacesRequest { Image = img, Attributes = ["ALL"] });
                    var n = 1;
                    foreach (var f in faces.FaceDetails ?? [])
                    {
                        var emotion = (f.Emotions ?? []).OrderByDescending(e => e.Confidence).FirstOrDefault();
                        var details = $"Age: {f.AgeRange?.Low}-{f.AgeRange?.High}, " +
                                      $"Gender: {f.Gender?.Value}, " +
                                      $"Emotion: {emotion?.Type}, " +
                                      $"Smile: {YesNo(f.Smile?.Value)}, " +
                                      $"Eyeglasses: {YesNo(f.Eyeglasses?.Value)}, " +
                                      $"Beard: {YesNo(f.Beard?.Value)}";
                        result.Items.Add(new($"Face {n}", f.Confidence ?? 0, details));
                        result.Boxes.Add(ToBox(f.BoundingBox, $"Face {n}"));
                        n++;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unknown feature: {feature}");
            }

            return result;
        }

        private static Box ToBox(BoundingBox? b, string label) =>
            new(b?.Left ?? 0, b?.Top ?? 0, b?.Width ?? 0, b?.Height ?? 0, label);

        private static string YesNo(bool? value) => value == true ? "Yes" : "No";
    }
}
