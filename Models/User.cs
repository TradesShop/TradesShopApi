using System;

namespace TradePlatform.Api.Models;

public class User
{
	public Guid id { get; set; }

	public string firstname { get; set; }

	public string lastname { get; set; }

	public string? country_code { get; set; }

	public string email { get; set; }

	public string? password_hash { get; set; }

	public string phone { get; set; }

	public int? user_type { get; set; }

	public string? utype_code { get; set; }

	public bool is_active { get; set; }

	public DateTime created_at { get; set; }

	public DateTime updated_at { get; set; }

	public string? stripe_customer_id { get; set; }

	public bool? verified { get; set; }

	public bool? exists { get; set; }

	public Guid? customer_id { get; set; }

	public Guid? business_id { get; set; }

	public string? jwttoken { get; set; }

	public string? verifycode { get; set; }
}
