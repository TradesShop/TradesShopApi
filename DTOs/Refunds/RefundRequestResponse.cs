using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundRequestResponse : RefundRequestMetaDto
{
	public int entity_type_id { get; set; }

	public Guid entity_id { get; set; }

	public string? reason { get; set; }

	public int status_id { get; set; }

	public string status { get; set; }

	public Guid? created_by { get; set; }
}
