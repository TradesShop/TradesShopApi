using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundRequestListDto : RefundRequestMetaDto
{
	public string business_name { get; set; }

	public string email { get; set; }

	public int entity_type_id { get; set; }

	public string entity_type { get; set; }

	public Guid entity_id { get; set; }

	public int status_id { get; set; }

	public string status { get; set; }

	public string reason { get; set; }

	public string plan_type { get; set; }

	public Guid? refund_id { get; set; }
}
