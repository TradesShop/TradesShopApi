using Azure;

using Azure.AI.Vision.ImageAnalysis;

namespace TradePlatform.Api.Services.Azure_OCR
{
    public class AzureVisionService
    {
        private readonly string _endpoint;
        private readonly string _key;
        private readonly HttpClient _http;

        public AzureVisionService(IConfiguration config)
        {
            _endpoint = config["AzureVision:Endpoint"];
            _key = config["AzureVision:Key"];
            _http = new HttpClient();
        }
        // ⭐ NEW: Stream-based OCR (for verify-blob)
        public async Task<string> ExtractTextFromStreamAsync(Stream stream)
        {
            // 1. Read stream into bytes
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // 2. Convert to BinaryData
            var binary = BinaryData.FromBytes(bytes);

            // 3. Create the Vision client (same as your URL method)
            var client = new ImageAnalysisClient(
                new Uri(_endpoint),
                new AzureKeyCredential(_key)
            );

            // 4. Analyse the image for OCR
            var result = await client.AnalyzeAsync(
                binary,
                VisualFeatures.Read
            );

            var read = result.Value.Read;

            if (read == null || read.Blocks == null)
                return string.Empty;

            // 5. Extract text
            var text = string.Join("\n",
                read.Blocks
                    .SelectMany(b => b.Lines)
                    .Select(l => l.Text)
            );

            return text;
        }

        public async Task<string> ExtractTextAsync(string imageUrl)
        {
            // 1. Download the image bytes
            var bytes = await _http.GetByteArrayAsync(imageUrl);
            var binary = BinaryData.FromBytes(bytes);

            // 2. Create the Vision client
            var client = new ImageAnalysisClient(
                new Uri(_endpoint),
                new AzureKeyCredential(_key)
            );

            // 3. Analyse the image for OCR (Read)
            var result = await client.AnalyzeAsync(
                binary,
                VisualFeatures.Read
            );

            var read = result.Value.Read;

            if (read == null || read.Blocks == null)
                return string.Empty;

            // 4. Extract text from blocks → lines → text
            var text = string.Join("\n",
                read.Blocks
                    .SelectMany(b => b.Lines)
                    .Select(l => l.Text)
            );

            return text;
        }
    }
}
