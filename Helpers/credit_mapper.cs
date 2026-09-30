using System.Collections.Generic;
using System.Text.Json;
using TradePlatform.Api.DTOs.Credits;

namespace TradePlatform.Api.Helpers;

public static class credit_mapper
{
	public static credit_transaction_dto Map(dynamic tx)
	{
		string type = tx.type;
		string source_type = tx.source_type;
		int credits = tx.credits;
		string metadata = tx.metadata;
		Dictionary<string, object> meta = (string.IsNullOrEmpty(metadata) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(metadata));
		string label = type switch
		{
			"used" => "Credits Used", 
			"earned" => "Credits Earned", 
			"refund" => "Credits Refunded", 
			"bonus" => "Bonus Credits", 
			"adjustment" => "Admin Adjustment", 
			"purchase" => "Credits Purchased", 
			_ => type, 
		};
		string description = source_type switch
		{
			"job_posts" => meta.ContainsKey("job_title") ? $"Job Purchased — {meta["job_title"]}" : "Job Purchased", 
			"subscription" => "Subscription Credits", 
			"credit_bundle" => "Purchased Bundle Credits", 
			"addons" => meta.ContainsKey("addon_name") ? $"Addon — {meta["addon_name"]}" : "Addon Purchase", 
			"system" => "System Generated", 
			"admin" => meta.ContainsKey("reason") ? $"Admin Adjustment — {meta["reason"]}" : "Admin Adjustment", 
			_ => source_type ?? "Unknown Source", 
		};
		credit_transaction_dto credit_transaction_dto2 = new credit_transaction_dto();
		credit_transaction_dto2.label = label;
		credit_transaction_dto2.credits = credits;
		credit_transaction_dto2.description = description;
		credit_transaction_dto2.reason = (meta.ContainsKey("reason") ? meta["reason"].ToString() : null);
		credit_transaction_dto2.job_title = (meta.ContainsKey("job_title") ? meta["job_title"].ToString() : null);
		credit_transaction_dto2.addon_name = (meta.ContainsKey("addon_name") ? meta["addon_name"].ToString() : null);
		credit_transaction_dto2.created_at = tx.created_at;
		credit_transaction_dto2.id = tx.id;
		return credit_transaction_dto2;
	}
}
