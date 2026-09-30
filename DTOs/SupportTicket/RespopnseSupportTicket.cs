using System;

namespace TradePlatform.Api.DTOs.SupportTicket;

public class RespopnseSupportTicket
{
	public bool success { get; set; }

	public Guid? master_ticket_id { get; set; }

	public Guid? support_ticket_id { get; set; }

	public string? ticket_number { get; set; }

	public string? message { get; set; }
}
