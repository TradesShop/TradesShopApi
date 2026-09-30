using System;

namespace TradePlatform.Api.DTOs.SupportTicket;

public class CreateSupportTicketDto
{
	public Guid? user_id { get; set; }

	public int? user_kind { get; set; }

	public int? user_type { get; set; }

	public string? email { get; set; }

	public string? phone_number { get; set; }

	public string? full_name { get; set; }

	public string country_code { get; set; }

	public int? category_id { get; set; }

	public string subject { get; set; }

	public string message_body { get; set; }

	public string? ip_address { get; set; }
}
