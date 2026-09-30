using System;

namespace TradePlatform.Api.DTOs.Reviews;

public class ReviewReplySubmit
{
	public Guid user_id { get; set; }

	public Guid review_id { get; set; }

	public string reply_text { get; set; }
}
