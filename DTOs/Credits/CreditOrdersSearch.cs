using System;

namespace TradePlatform.Api.DTOs.Credits;

public class CreditOrdersSearch
{
	public Guid user_id { get; set; }

	public Guid? target_user_id { get; set; }

	public int page_number { get; set; } = 1;

	public int page_size { get; set; } = 20;
}
public class CreditOrderUpdateDto
{
    public Guid id { get; set; }
    public Guid? user_id { get; set; }
    public Guid? target_user_id { get; set; }
    public bool is_refund_requested { get; set; } = false;
    public string? cancellation_reason { get; set; }  
    public string? actor { get; set; } = "user";
    public string? source { get; set; } = "web_portal";
    public string? metadata_json { get; set; }
}

  