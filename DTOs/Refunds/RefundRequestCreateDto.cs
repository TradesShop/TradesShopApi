using System;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundRequestCreateDto
{
	public Guid? user_id { get; set; }
	public Guid? target_user_id { get; set; }
	public int entity_type_id { get; set; }
	public Guid entity_id { get; set; }
	public string? reason { get; set; }
	public Guid? created_by { get; set; }
    public string status_code { get; set; } = "pending";
	public bool is_refund_requested { get; set; } = true;
}
    
