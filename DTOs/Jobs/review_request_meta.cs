using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class review_request_meta : review_reply_meta
{
	public bool? success { get; set; }

	public string? message { get; set; }

	public Guid? review_request_id { get; set; }

	public Guid? review_id { get; set; }

	public string? review_title { get; set; }

	public int? review_rating { get; set; }

	public string? review_text { get; set; }

	public DateTime? review_at { get; set; }

	public string? reviewer_first { get; set; }

	public string? reviewer_last { get; set; }
}
