using System;

namespace TradePlatform.Api.DTOs;

public class FileUploadRequestDto
{
	public Guid? user_id { get; set; }

	public string? filename { get; set; }

	public string? file_url { get; set; }

	public string? read_url { get; set; }

	public string? upload_url { get; set; }

	public string? filetype { get; set; }

	public int? size_kb { get; set; } = 0;

	public Guid entity_id { get; set; }

	public int entity_type { get; set; }

	public string upload_type { get; set; }

	public string content_type { get; set; }

	public string? work_stage { get; set; }

	public bool is_primary { get; set; }
}
