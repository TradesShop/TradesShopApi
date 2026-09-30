using System;

namespace TradePlatform.Api.DTOs.Files;

public class UploadFilesDto
{
	public Guid id { get; set; }

	public string file_name { get; set; }

	public string file_url { get; set; }

	public string? file_type { get; set; }

	public int? file_size { get; set; }

	public string description { get; set; }

	public string? work_stage { get; set; }

	public DateTime created_at { get; set; }
}
