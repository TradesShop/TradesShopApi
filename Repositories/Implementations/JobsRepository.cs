using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Helpers;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Models.Jobs;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.BackgroundEmailQueue;

namespace TradePlatform.Api.Repositories.Implementations;

public class JobsRepository : IJobsRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	private readonly IBackgroundEmailQueue _backgroundEmailQueue;

	public JobsRepository(DapperContext context, IIdentityService identity, IBackgroundEmailQueue backgroundEmailQueue)
	{
		_context = context;
		_identity = identity;
		_backgroundEmailQueue = backgroundEmailQueue;
	}

	private string mask_phone(string phone)
	{
		if (string.IsNullOrWhiteSpace(phone))
		{
			return "";
		}
		int prefixLength = Math.Min(3, phone.Length);
		string prefix = phone.Substring(0, prefixLength);
		int maskLength = Math.Max(4, phone.Length - prefixLength);
		return prefix + new string('*', maskLength);
	}

	private string mask_email(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			return "";
		}
		string[] parts = email.Split('@');
		if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
		{
			return "****";
		}
		string name = parts[0];
		string domain = parts[1];
		string maskedName = name.Length switch
		{
			1 => name + "***", 
			2 => name.Substring(0, 1) + "***" + name.Substring(1), 
			_ => name.Substring(0, 2) + new string('*', Math.Max(3, name.Length - 2)), 
		};
		int lastDotIndex = domain.LastIndexOf('.');
		string maskedDomain;
		if (lastDotIndex > 0)
		{
			string domainName = domain.Substring(0, lastDotIndex);
			string extension = domain.Substring(lastDotIndex);
			string visibleDomainPrefix = ((domainName.Length > 1) ? domainName.Substring(0, 1) : domainName);
			maskedDomain = visibleDomainPrefix + "***" + extension;
		}
		else
		{
			maskedDomain = domain.Substring(0, 1) + "***";
		}
		return maskedName + "@" + maskedDomain;
	}

	private string mask_customer(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return "";
		}
		return name.Substring(0, 2) + "****";
	}

	public async Task<JobPostResponseDto> CreateJobPostAsync(JobPostRequestDto request)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DataTable table = new DataTable();
		table.Columns.Add("question_id", typeof(int));
		table.Columns.Add("answer_id", typeof(int));
		table.Columns.Add("answer_text", typeof(string));
		foreach (JobPostAnswerDto item in request.answers)
		{
			IEnumerable<int> list = item.answer_ids;
			if (list != null)
			{
				foreach (int a in list)
				{
					table.Rows.Add(item.question_id, a, DBNull.Value);
				}
				continue;
			}
			int? answer_id = item.answer_id;
			if (answer_id.HasValue)
			{
				int single = answer_id.GetValueOrDefault();
				table.Rows.Add(item.question_id, single, DBNull.Value);
			}
		}
		string workplacejson = JsonConvert.SerializeObject(request.workplace);
		Guid user_id = _identity.GetUserId();
		var parameters = new
		{
			user_id = user_id,
			category_id = request.category_id,
			firstname = request.firstname,
			lastname = request.lastname,
			phone = request.phone,
			email = request.email,
			title = request.title,
			description = request.description,
			timeline_id = request.timeline_id,
			budget_range_id = request.budget_range_id,
			postcode = request.postcode,
			country_code = request.country_code,
			latitude = request.latitude,
			longitude = request.longitude,
			created_by = _identity.GetUserId(),
			ip_address = _identity.GetIpAddress(),
			user_agent = _identity.GetUserAgent(),
			workplace = workplacejson,
			answers = table.AsTableValuedParameter("JobPostAnswerType")
		};
		CommandType? commandType = CommandType.StoredProcedure;
		JobPostResponseDto result = await conn.QueryFirstOrDefaultAsync<JobPostResponseDto>("usp_job_posts_create_async", parameters, null, null, commandType);
		JobPostEmailNotifyDto job_post_notify = new JobPostEmailNotifyDto
		{
			user_id = user_id,
			job_id = result.id,
			customer_email = request.email,
			customer_name = request.firstname,
			is_customer = (_identity.GetUserType() == UserType.customer)
		};
		await _backgroundEmailQueue.QueueJobPostedEmailAsync(job_post_notify);
		return result;
	}

	public async Task<IEnumerable<Job>> GetJobsByUserTradesAndLocation(tUserJobsListRequestDto tUserJobsReq)
	{
		using IDbConnection connection = _context.CreateConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", tUserJobsReq.user_id);
		parameters.Add("@status_id", tUserJobsReq.status_id);
		parameters.Add("@last_created_at", tUserJobsReq.last_created_at);
		parameters.Add("@last_id", tUserJobsReq.last_id);
		parameters.Add("@limit", tUserJobsReq.limit);
		parameters.Add("@sort_key", tUserJobsReq.sort_key);
		parameters.Add("@sort_dir", tUserJobsReq.sort_dir);
		parameters.Add("@category_ids", tUserJobsReq.category_ids);
		parameters.Add("@location_ids", tUserJobsReq.location_ids);
		parameters.Add("@max_credits", tUserJobsReq.max_credits);
		parameters.Add("@max_distance", tUserJobsReq.max_distance);
		parameters.Add("@last_credit", tUserJobsReq.last_credit);
		parameters.Add("@last_distance", tUserJobsReq.last_distance);
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryAsync<Job>("usp_jobs_get_by_user_trades_and_location", parameters, null, null, commandType);
	}

	public async Task<IEnumerable<Job>> GetUserPurchasedJobsList(tUserJobsListRequestDto tUserJobsReq)
	{
		using IDbConnection connection = _context.CreateConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", tUserJobsReq.user_id);
		parameters.Add("@last_created_at", tUserJobsReq.last_created_at);
		parameters.Add("@last_id", tUserJobsReq.last_id);
		parameters.Add("@limit", tUserJobsReq.limit);
		parameters.Add("@sort_key", tUserJobsReq.sort_key);
		parameters.Add("@sort_dir", tUserJobsReq.sort_dir);
		parameters.Add("@category_ids", tUserJobsReq.category_ids);
		parameters.Add("@location_ids", tUserJobsReq.location_ids);
		parameters.Add("@max_credits", tUserJobsReq.max_credits);
		parameters.Add("@max_distance", tUserJobsReq.max_distance);
		parameters.Add("@last_credit", tUserJobsReq.last_credit);
		parameters.Add("@last_distance", tUserJobsReq.last_distance);
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryAsync<Job>("usp_jobs_get_purchased_list", parameters, null, null, commandType);
	}

	public async Task<IEnumerable<MyJobsResponseDto>> MyJobPostsGetAsync(MyJobsRequestDto myjobsReq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", myjobsReq.user_id);
		parameters.Add("@status_id", myjobsReq.status_id);
		parameters.Add("@last_created_at", myjobsReq.last_created_at);
		parameters.Add("@last_id", myjobsReq.last_id);
		parameters.Add("@limit", myjobsReq.limit);
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<MyJobsResponseDto>("usp_jobs_mylist_get_async", parameters, null, null, commandType);
	}

	public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_User_Async(Guid job_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = job_id,
			user_id = _identity.GetUserId()
		};
		CommandType? commandType = CommandType.StoredProcedure;
		using SqlMapper.GridReader multi = await conn.QueryMultipleAsync("usp_job_get_by_id_for_user", param, null, null, commandType);
		JobFullDetailsDto job = await multi.ReadFirstOrDefaultAsync<JobFullDetailsDto>();
		if (job == null)
		{
			return null;
		}
		job.Questions = JobQuestions.MapQuestions(await multi.ReadAsync<QuestionAnswerRow>());
		return job;
	}

	public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_tUser_Async(Guid job_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = job_id,
			user_id = _identity.GetUserId()
		};
		CommandType? commandType = CommandType.StoredProcedure;
		using SqlMapper.GridReader multi = await conn.QueryMultipleAsync("usp_job_get_by_id_for_tuser", param, null, null, commandType);
		JobFullDetailsDto job = await multi.ReadFirstOrDefaultAsync<JobFullDetailsDto>();
		if (job == null)
		{
			return null;
		}
		if (!job.job_purchase_id.HasValue || job.job_purchase_id == Guid.Empty)
		{
			job.phone = mask_phone(job.phone);
			job.email = mask_email(job.email);
		}
		job.Questions = JobQuestions.MapQuestions(await multi.ReadAsync<QuestionAnswerRow>());
		return job;
	}

	public async Task<PurchaseJobResultDto> JobPurchaseCreateAsync(PurchaseJobRequestDto pjrRequest)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = pjrRequest.user_id,
			job_id = pjrRequest.id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		PurchaseJobResultDto purchaseResult = await conn.QueryFirstOrDefaultAsync<PurchaseJobResultDto>("usp_job_purchase_create_async", param, null, null, commandType);
		if (purchaseResult == null)
		{
			return new PurchaseJobResultDto
			{
				success = false,
				message = "No response from database"
			};
		}
		purchaseResult.is_customer = _identity.GetUserType() == UserType.customer;
		await _backgroundEmailQueue.QueuePurchaseEmailAsync(pjrRequest.user_id, purchaseResult);
		return purchaseResult;
	}

	public async Task<DisputeJobResponse> DisputePurchaseJobSubmit(DisputePurchaseJob dpjRequest)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			dpjRequest.job_purchase_id, dpjRequest.job_id, dpjRequest.raised_by_user_id, dpjRequest.against_user_id, dpjRequest.dispute_type_id, dpjRequest.reason, dpjRequest.status_code, dpjRequest.resolution_notes, dpjRequest.refund_credits, dpjRequest.decision_id,
			dpjRequest.updated_by
		};
		CommandType? commandType = CommandType.StoredProcedure;
		DisputeJobResponse anydispute = await conn.QueryFirstOrDefaultAsync<DisputeJobResponse>("[dbo].[usp_job_dispute_insert]", param, null, null, commandType);
		NotifyEmailDataModel anyNotifyEmail = JobDisputeMappingHelper.ToNotifyEmailDataModel(anydispute);
		await _backgroundEmailQueue.QueueNotifyEmailForJobDisputeReceived(anyNotifyEmail);
		return anydispute;
	}

	public async Task<DisputeJobResponse> JobDisputeUpdateStatus(DisputeStatusRequest dsReq)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = dsReq.id,
			status_code = dsReq.status_code,
			updated_by = dsReq.user_id,
			is_admin = dsReq.is_admin
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<DisputeJobResponse>("[dbo].[usp_job_dispute_status_update]", param, null, null, commandType);
	}

	public async Task<DisputeJobResponse> DisputePurchaseJobUpsert(DisputePurchaseJob dpjRequest)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			dpjRequest.id, dpjRequest.job_purchase_id, dpjRequest.job_id, dpjRequest.raised_by_user_id, dpjRequest.against_user_id, dpjRequest.dispute_type_id, dpjRequest.reason, dpjRequest.status_id, dpjRequest.resolution_notes, dpjRequest.refund_credits,
			dpjRequest.decision_id, dpjRequest.updated_by
		};
		CommandType? commandType = CommandType.StoredProcedure;
		DisputeJobResponse anydispute = await conn.QueryFirstOrDefaultAsync<DisputeJobResponse>("[dbo].[usp_job_dispute_upsert]", param, null, null, commandType);
		if (dpjRequest.status_id == 21 && !dpjRequest.id.HasValue)
		{
			NotifyEmailDataModel anyNotifyEmail = JobDisputeMappingHelper.ToNotifyEmailDataModel(anydispute);
			await _backgroundEmailQueue.QueueNotifyEmailForJobDisputeReceived(anyNotifyEmail);
		}
		return anydispute;
	}

	public async Task<DisputeJobResponse> JobDisputeActionAsync(DisputeActionReq jdaDto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = jdaDto.id,
			status_id = jdaDto.status_id,
			priority_id = jdaDto.priority_id,
			decision_id = jdaDto.decision_id,
			resolution_notes = jdaDto.resolution_notes,
			refund_credits = jdaDto.refund_credits,
			user_id = _identity.GetUserId()
		};
		CommandType? commandType = CommandType.StoredProcedure;
		DisputeJobResponse disputeaction = await conn.QueryFirstOrDefaultAsync<DisputeJobResponse>("[dbo].[usp_job_dispute_admin_action]", param, null, null, commandType);
		if (jdaDto.send_email)
		{
			NotifyEmailDataModel anyNotifyEmail = JobDisputeMappingHelper.ToNotifyEmailDataModel(disputeaction);
			await _backgroundEmailQueue.QueueNotifyEmailForJobDisputeAction(anyNotifyEmail);
		}
		return disputeaction;
	}

	public async Task<DisputeJobResponse> DisputeJobGetByDisputeId(Guid dispute_id, Guid? user_id)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = dispute_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<DisputeJobResponse>("[dbo].[usp_job_dispute_get_by_id]", param, null, null, commandType);
	}

	public async Task<IEnumerable<InterestedTradersDto>> GetInterestedTradersByJobId(Guid job_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { job_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryAsync<InterestedTradersDto>("usp_job_interested_trades_by_jobid", param, null, null, commandType);
	}

	public async Task<CommonResponseDto> UpdateJobStatusAsync(JobUpdateStatus statDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			job_id = statDto.job_id,
			statuscode = statDto.statuscode,
			user_id = _identity.GetUserId(),
			closure_reason_id = statDto.closure_reason_id,
			completed_by = statDto.completed_by,
			note = statDto.note,
			ip_address = _identity.GetIpAddress(),
			user_agent = _identity.GetUserAgent()
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<CommonResponseDto>("usp_job_post_update_status", param, null, null, commandType);
	}

	public async Task UpdateJobEntityAsync(JobUpdateEntityDto jueDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			job_id = jueDto.job_id,
			user_id = _identity.GetUserId(),
			action = jueDto.action,
			title = jueDto.title,
			description = jueDto.description,
			timeline_id = jueDto.timeline_id,
			budget_range_id = jueDto.budget_range_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.QueryAsync("usp_job_post_update_entity", param, null, null, commandType);
	}

	public async Task<JobContactDto> UpdateAnyContactAsync(JobContactDto jcDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { jcDto.contact_id, jcDto.job_id, jcDto.user_id, jcDto.firstname, jcDto.lastname, jcDto.email, jcDto.phone };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<JobContactDto>("usp_job_contact_upsert", param, null, null, commandType);
	}

	public async Task<JobLocationDto> UpdateJobLocationAsync(JobLocationDto jlDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			job_id = jlDto.job_id,
			user_id = jlDto.user_id,
			address_line1 = jlDto.address_line1,
			address_line2 = jlDto.address_line2,
			city = jlDto.city,
			postcode = jlDto.postcode,
			country_code = jlDto.country_code,
			latitude = jlDto.latitude,
			longitude = jlDto.longitude,
			workplace = jlDto.workplace,
			silent = false,
			job_posts_update = true
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<JobLocationDto>("[dbo].[usp_job_location_upsert]", param, null, null, commandType);
	}

	public async Task<NotifyEmailDataModel> JobPurchaseNotifyEmailDetails(PurchaseJobResultDto jp_dto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { jp_dto.job_id, jp_dto.job_purchase_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<NotifyEmailDataModel>("[dbo].[usp_job_purchase_notify_email_details]", param, null, null, commandType);
	}

	public async Task<IEnumerable<TraderDto>> JobPostNotifyEmailDetailsForTraders(Guid job_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { job_id };
		CommandType? commandType = CommandType.StoredProcedure;
		using SqlMapper.GridReader multi = await connection.QueryMultipleAsync("dbo.usp_job_post_trader_notify_email_details", param, null, null, commandType);
		PostedJobDto postedJob = await multi.ReadSingleAsync<PostedJobDto>();
		List<TraderDto> traders = (await multi.ReadAsync<TraderDto>()).ToList();
		List<RecentJobDto> recentJobs = (await multi.ReadAsync<RecentJobDto>()).ToList();
		ILookup<Guid, RecentJobDto> jobsLookup = recentJobs.ToLookup((RecentJobDto j) => j.trader_id);
		foreach (TraderDto trader in traders)
		{
			trader.PostedJob = postedJob;
			trader.RecentJobs = jobsLookup[trader.trader_id].OrderByDescending((RecentJobDto j) => j.created_at).ToList();
		}
		return traders;
	}

	public async Task JobNotificationUpsertAsync(JobNotificationUpsertReq jnReq)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { jnReq.job_id, jnReq.user_id, jnReq.notification_type_id, jnReq.is_view };
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.QueryAsync("dbo.usp_job_notification_insert_event", param, null, null, commandType);
	}

	public async Task<IEnumerable<DisputeTraders>> GetDisputeTradersByJobIdAsync(Guid job_id)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new { job_id };
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryAsync<DisputeTraders>("[dbo].[usp_job_dispute_traders_get_by_job_id]", param, null, null, commandType);
	}

	public async Task<Job_meta> AssignTradeAsync(JobAssignTradeRequest jat_req)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			job_id = jat_req.job_id,
			job_purchase_id = jat_req.job_purchase_id,
			assigned_by_user_id = jat_req.user_id
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<Job_meta>("[dbo].[usp_job_assign_trade]", param, null, null, commandType);
	}
}
