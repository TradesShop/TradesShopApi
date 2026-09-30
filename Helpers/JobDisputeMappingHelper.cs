using System;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models.Email;

namespace TradePlatform.Api.Helpers;

public static class JobDisputeMappingHelper
{
	public static NotifyEmailDataModel ToNotifyEmailDataModel(DisputeJobResponse djr)
	{
		if (djr == null)
		{
			throw new ArgumentNullException("djr");
		}
		return new NotifyEmailDataModel
		{
			job_id = djr.job_id,
			job_ref = djr.job_ref,
			receiver_id = djr.receiver_id,
			receiver_name = djr.receiver_name,
			receiver_email = djr.receiver_email,
			receive_important = djr.receive_important,
			job_dispute_id = djr.job_dispute_id,
			job_purchase_id = djr.job_purchase_id,
			job_title = djr.job_title,
			job_created_at = djr.job_created_at,
			dispute_created_at = djr.dispute_created_at,
			dispute_decision = djr.dispute_decision,
			refund_credits = djr.refund_credits,
			resolution_notes = djr.resolution_notes
		};
	}
}
