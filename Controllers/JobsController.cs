using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models;
using TradePlatform.Api.Services.Jobs;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : BaseController
{
	private readonly IJobsService _jobsService;

	public JobsController(IJobsService jobsService)
	{
		_jobsService = jobsService;
	}

	[HttpPost("post")]
	public async Task<IActionResult> CreateAJobPostAsync([FromBody] JobPostRequestDto jPostDtos)
	{
		return ApiOk(await _jobsService.CreateJobPostAsync(jPostDtos));
	}

	[HttpPost("myjobs")]
	public async Task<IActionResult> myjobs([FromBody] MyJobsRequestDto myjobsDtos)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		myjobsDtos.user_id = user_id;
		return ApiOk(await _jobsService.MyJobPostsGetAsync(myjobsDtos));
	}

	[HttpPost("list")]
	public async Task<IActionResult> GetRecommendedJobs([FromBody] tUserJobsListRequestDto tUserJobsReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		tUserJobsReq.user_id = ResolveEffectiveUser(callerId, callerType, tUserJobsReq?.target_user_id);
		return ApiOk(await _jobsService.GetJobsByUserTradesAndLocation(tUserJobsReq));
	}

	[HttpPost("purchased")]
	public async Task<IActionResult> GetUserPurchasedJobs([FromBody] tUserJobsListRequestDto tUserJobsReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid callerId = identity.userId;
		UserType callerType = identity.userType;
		tUserJobsReq.user_id = ResolveEffectiveUser(callerId, callerType, tUserJobsReq?.target_user_id);
		return ApiOk(await _jobsService.GetUserPurchasedJobsList(tUserJobsReq));
	}

	[HttpPost("purchase")]
	public async Task<IActionResult> PurchaseJob([FromBody] PurchaseJobRequestDto pjrRequest)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		pjrRequest.user_id = user_id;
		PurchaseJobResultDto result = await _jobsService.JobPurchaseCreateAsync(pjrRequest);
		if (!result.success)
		{
			return ApiError(result);
		}
		return ApiOk(await _jobsService.GetJobDetailsByIdFor_tUser_Async(pjrRequest.id));
	}

	[HttpGet("{job_id}")]
	public async Task<IActionResult> GetJobDetailsByIdFor_tUser_Async(Guid job_id)
	{
		return ApiOk(await _jobsService.GetJobDetailsByIdFor_tUser_Async(job_id));
	}

	[HttpGet("myjobs/{job_id}")]
	public async Task<IActionResult> GetJobDetailsByIdFor_User_Async(Guid job_id)
	{
		return ApiOk(await _jobsService.GetJobDetailsByIdFor_User_Async(job_id));
	}

	[HttpGet("interested_trades/{job_id}")]
	public async Task<IActionResult> GetInterestedTradersByJobId(Guid job_id)
	{
		return ApiOk(await _jobsService.GetInterestedTradersByJobId(job_id));
	}

	[HttpPost("update/status")]
	public async Task<IActionResult> UpdateJobStatusAsync([FromBody] JobUpdateStatus statDto)
	{
		return ApiOk(await _jobsService.UpdateJobStatusAsync(statDto));
	}

	[HttpPost("update/entity")]
	public async Task<IActionResult> UpdateJobEntityAsync([FromBody] JobUpdateEntityDto jueDto)
	{
		await _jobsService.UpdateJobEntityAsync(jueDto);
		return ApiOk(new
		{
			success = true
		});
	}

	[HttpPost("contact/upsert")]
	public async Task<IActionResult> UpdateAnyContactAsync(JobContactDto jcDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		jcDto.user_id = user_id;
		JobContactDto anyresult = await _jobsService.UpdateAnyContactAsync(jcDto);
		if (anyresult == null)
		{
			return ApiError(anyresult);
		}
		return ApiOk(anyresult);
	}

	[HttpPost("location/upsert")]
	public async Task<IActionResult> UpdateJobLocationAsync(JobLocationDto jcDto)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		jcDto.user_id = user_id;
		JobLocationDto anyresult = await _jobsService.UpdateJobLocationAsync(jcDto);
		if (anyresult == null)
		{
			return ApiError(anyresult);
		}
		return ApiOk(anyresult);
	}

	[HttpPost("notifications")]
	public async Task<IActionResult> JobNotificationUpsertAsync([FromBody] JobNotificationUpsertReq jnReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		var (user_id, _) = identity;
		_ = identity.userType;
		jnReq.user_id = user_id;
		await _jobsService.JobNotificationUpsertAsync(jnReq);
		return ApiOk();
	}

	[HttpPost("{job_id}/assign-trade")]
	public async Task<IActionResult> AssignTrade(Guid job_id, [FromBody] JobAssignTradeRequest jat_req)
	{
		try
		{
			(Guid userId, UserType userType) identity = GetIdentity();
			var (user_id, _) = identity;
			_ = identity.userType;
			if (jat_req.job_id != job_id)
			{
				return BadRequest("Invalid job ID.");
			}
			jat_req.user_id = user_id;
			return ApiOk(await _jobsService.AssignTradeAsync(jat_req));
		}
		catch (Exception ex)
		{
			return BadRequest(new
			{
				message = ex.Message
			});
		}
	}

	[HttpGet("{job_id}/disputes/traders")]
	public async Task<IActionResult> GetDisputeTradersByJobIdAsync(Guid job_id)
	{
		return ApiOk(await _jobsService.GetDisputeTradersByJobIdAsync(job_id));
	}

	[HttpPost("dispute/submit")]
	public async Task<IActionResult> DisputePurchaseJobSubmit([FromBody] DisputePurchaseJob dpjReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		dpjReq.raised_by_user_id = ResolveEffectiveUser(user_id, user_type, dpjReq?.raised_by_user_id);
		dpjReq.updated_by = user_id;
		return ApiOk(await _jobsService.DisputePurchaseJobSubmit(dpjReq));
	}

	[HttpPost("dispute")]
	public async Task<IActionResult> DisputePurchaseJobAsync([FromBody] DisputePurchaseJob dpjReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		dpjReq.raised_by_user_id = ResolveEffectiveUser(user_id, user_type, dpjReq?.raised_by_user_id);
		dpjReq.updated_by = user_id;
		return ApiOk(await _jobsService.DisputePurchaseJobUpsert(dpjReq));
	}

	[HttpGet("dispute/{dispute_id}/{target_user_id?}")]
	public async Task<IActionResult> DisputeJobGetByDisputeId(Guid dispute_id, Guid? target_user_id)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		user_id = ResolveEffectiveUser(user_id, user_type, target_user_id);
		return ApiOk(await _jobsService.DisputeJobGetByDisputeId(dispute_id, user_id));
	}

	[HttpPut("dispute/{dispute_id}/status")]
	public async Task<IActionResult> JobDisputeUpdateStatus(Guid dispute_id, [FromBody] DisputeStatusRequest dsReq)
	{
		(Guid userId, UserType userType) identity = GetIdentity();
		Guid user_id = identity.userId;
		UserType user_type = identity.userType;
		dsReq.user_id = ResolveEffectiveUser(user_id, user_type, dsReq.target_user_id);
		dsReq.id = dispute_id;
		return ApiOk(await _jobsService.JobDisputeUpdateStatus(dsReq));
	}
}
