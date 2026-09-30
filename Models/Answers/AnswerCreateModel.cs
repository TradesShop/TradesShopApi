using System;

namespace TradePlatform.Api.Models.Answers;

public class AnswerCreateModel
{
	public int answer_group_id { get; set; }

	public string title { get; set; }

	public string description { get; set; }

	public bool isactive { get; set; }

	public bool hascredit { get; set; }

	public int credit { get; set; }

	public Guid? updated_by { get; set; }
}
