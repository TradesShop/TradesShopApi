using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.OpenCage;

public sealed class OpenCageResponse
{
	public List<OpenCageResult>? Results { get; set; }
}
