using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.Configuration;

namespace TradePlatform.Api.Services.Google_OCR;

public class GoogleVisionService
{
	private readonly ImageAnnotatorClient _client;

	private readonly HttpClient _http;

	public GoogleVisionService(IConfiguration config)
	{
		Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", config["GoogleVision:CredentialsPath"]);
		_client = ImageAnnotatorClient.Create();
	}

	public async Task<string> ExtractTextFromStreamAsync(Stream stream)
	{
		Image image = Image.FromStream(stream);
		ImageAnnotatorClient client = ImageAnnotatorClient.Create();
		IReadOnlyList<EntityAnnotation> response = await client.DetectTextAsync(image);
		if (response == null || response.Count == 0)
		{
			return string.Empty;
		}
		return string.Join("\n", response.Select((EntityAnnotation r) => r.Description));
	}

	public async Task<string> ExtractTextAsync(string imageUrl)
	{
		Image image = Image.FromBytes(await _http.GetByteArrayAsync(imageUrl));
		ImageAnnotatorClient client = ImageAnnotatorClient.Create();
		IReadOnlyList<EntityAnnotation> response = await client.DetectTextAsync(image);
		if (response == null || response.Count == 0)
		{
			return string.Empty;
		}
		return string.Join("\n", response.Select((EntityAnnotation r) => r.Description));
	}
}
