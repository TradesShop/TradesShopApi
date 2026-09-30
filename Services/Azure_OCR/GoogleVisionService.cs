using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;

namespace TradePlatform.Api.Services.Azure_OCR;

public class GoogleVisionService
{
	private readonly string _endpoint;

	private readonly string _key;

	private readonly HttpClient _http;

	public GoogleVisionService(IConfiguration config)
	{
		_endpoint = config["AzureVision:Endpoint"];
		_key = config["AzureVision:Key"];
		_http = new HttpClient();
	}

	public async Task<string> ExtractTextFromStreamAsync(Stream stream)
	{
		using MemoryStream ms = new MemoryStream();
		await stream.CopyToAsync(ms);
		byte[] bytes = ms.ToArray();
		BinaryData binary = BinaryData.FromBytes(bytes);
		ImageAnalysisClient client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));
		ReadResult read = (await client.AnalyzeAsync(binary, VisualFeatures.Read)).Value.Read;
		if (read == null || read.Blocks == null)
		{
			return string.Empty;
		}
		return string.Join("\n", from l in read.Blocks.SelectMany((DetectedTextBlock b) => b.Lines)
			select l.Text);
	}

	public async Task<string> ExtractTextAsync(string imageUrl)
	{
		BinaryData binary = BinaryData.FromBytes(await _http.GetByteArrayAsync(imageUrl));
		ImageAnalysisClient client = new ImageAnalysisClient(new Uri(_endpoint), new AzureKeyCredential(_key));
		ReadResult read = (await client.AnalyzeAsync(binary, VisualFeatures.Read)).Value.Read;
		if (read == null || read.Blocks == null)
		{
			return string.Empty;
		}
		return string.Join("\n", from l in read.Blocks.SelectMany((DetectedTextBlock b) => b.Lines)
			select l.Text);
	}
}
