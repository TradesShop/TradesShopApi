using System;
using System.Collections.Generic;

namespace TradePlatform.Api.DTOs.Refunds;

public class RefundsReqSearch
{
	public List<int> status_ids { get; set; } = new List<int>();

	public DateTime? date_from { get; set; }

	public DateTime? date_to { get; set; }

	public string? search { get; set; }

	public string? sort_by { get; set; }

	public int? page_number { get; set; }

	public int? page_size { get; set; }
}
