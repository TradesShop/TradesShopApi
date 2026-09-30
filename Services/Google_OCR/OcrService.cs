using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;

namespace TradePlatform.Api.Services.Google_OCR;

public class OcrService
{
	private readonly ImageAnnotatorClient _client;

	public OcrService(ImageAnnotatorClient client = null)
	{
		_client = client ?? ImageAnnotatorClient.Create();
	}

	public async Task<string> ExtractTextFromStreamAsync(Stream stream)
	{
		if (stream == null || stream.Length == 0L)
		{
			return string.Empty;
		}
		if (stream.CanSeek && stream.Position != 0L)
		{
			stream.Position = 0L;
		}
		Image image = await Image.FromStreamAsync(stream);
		TextAnnotation response = await _client.DetectDocumentTextAsync(image);
		if (response == null || string.IsNullOrWhiteSpace(response.Text))
		{
			return string.Empty;
		}
		return response.Text;
	}
}
