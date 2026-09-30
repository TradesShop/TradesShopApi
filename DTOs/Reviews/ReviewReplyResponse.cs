using System;
using TradePlatform.Api.DTOs.Jobs;

namespace TradePlatform.Api.DTOs.Reviews;

public class ReviewReplyResponse : review_reply_meta
{
	public Guid user_id { get; set; }

	public new int reply_id { get; set; }

	public Guid review_id { get; set; }

	public new string reply_text { get; set; }

	public bool success { get; set; }

	public string message { get; set; }
}
