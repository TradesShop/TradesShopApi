using System;

namespace TradePlatform.Api.DTOs;

public class FileDeleteRequestDto
{
	public Guid id { get; set; }

	public string file_name { get; set; }
}
