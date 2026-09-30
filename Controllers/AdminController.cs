using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.DTOs.Admin;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.PlanPrices;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Categories;
using TradePlatform.Api.Services.Jobs;
using TradePlatform.Api.Services.plans;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : BaseController
{
	private readonly IAdminJobsRepository _service;

	private readonly IJobsService _jobsService;

	private readonly ICategoryService _ctgryService;

	private readonly PlansService _plansService;

	public AdminController(IAdminJobsRepository service, IJobsService jobsService, ICategoryService ctgryService, PlansService plansService)
	{
		_service = service;
		_jobsService = jobsService;
		_ctgryService = ctgryService;
		_plansService = plansService;
	}

	[HttpPost("jobs/search")]
	public async Task<IActionResult> JobsSearch([FromBody] JobPostsSearchReq req)
	{
		return ApiOk(await _service.SearchJobsAsync(req));
	}

	[HttpPost("jobs/disputed")]
	public async Task<IActionResult> DisputedJobsAsync([FromBody] JobPostsSearchReq req)
	{
		return ApiOk(await _service.DisputedJobsAsync(req));
	}

	[HttpPost("jobs/dispute-requested")]
	public async Task<IActionResult> DisputeRequestedJobsAsync([FromBody] JobPostsSearchReq req)
	{
		return ApiOk(await _service.DisputeRequestedJobsAsync(req));
	}

	[HttpPost("jobs/purchased")]
	public async Task<IActionResult> PurchasedJobsAsync([FromBody] JobPostsSearchReq req)
	{
		return ApiOk(await _service.PurchasedJobsAsync(req));
	}

	[HttpPost("jobs/notpurchased")]
	public async Task<IActionResult> NotPurchasedJobsAsync([FromBody] JobPostsSearchReq req)
	{
		return ApiOk(await _service.NotPurchasedJobsAsync(req));
	}

	[HttpGet("jobs/get/{job_id}")]
	public async Task<IActionResult> GetJobDetailsByIdFor_admin_Async(Guid job_id)
	{
		return ApiOk(await _service.GetJobDetailsByIdFor_admin_Async(job_id));
	}

	[HttpPost("jobs/update")]
	public async Task<IActionResult> UpdateJobAsync([FromBody] JobUpdateDto jueDto)
	{
		await _service.UpdateJobAsync(jueDto);
		return ApiOk(new
		{
			success = true
		});
	}

	[HttpPost("jobs/dispute/action")]
	public async Task<IActionResult> JobDisputeActionAsync([FromBody] DisputeActionReq jdaDto)
	{
		return ApiOk(await _jobsService.JobDisputeActionAsync(jdaDto));
	}

	[HttpPut("category/update/{id:int}")]
	public async Task<IActionResult> CategoryUpsertAsync([FromRoute] int id, [FromBody] categories ctgry)
	{
		if (ctgry == null)
		{
			return BadRequest("Invalid category payload.");
		}
		ctgry.id = id;
		return ApiOk(await _ctgryService.CategoryUpsertAsync(ctgry));
	}

	[HttpPost("category/create")]
	public async Task<IActionResult> CategoryUpsertAsync([FromBody] categories ctgry)
	{
		if (ctgry == null)
		{
			return BadRequest("Invalid category payload.");
		}
		ctgry.id = null;
		return ApiOk(await _ctgryService.CategoryUpsertAsync(ctgry));
	}

	[HttpGet("plans")]
	public async Task<IActionResult> GetActivePlans([FromQuery] string plan_type, [FromQuery] Guid? user_id)
	{
		if (!user_id.HasValue || user_id.Value == Guid.Empty)
		{
			return ApiError(null, "A valid user_id is required.");
		}
		return ApiOk(await _plansService.GetActivePlansAsync(plan_type, user_id.Value));
	}

	[HttpPut("plans/update/{id}")]
	public async Task<IActionResult> PlansUpdateAsync([FromRoute] Guid id, [FromBody] PlansMeta anyplan)
	{
		anyplan.id = id;
		return ApiOk(await _plansService.PlansUpsertAsync(anyplan));
	}

	[HttpPost("plans/create")]
	public async Task<IActionResult> PlansCreateAsync([FromBody] PlansMeta anyplan)
	{
		anyplan.id = null;
		return ApiOk(await _plansService.PlansUpsertAsync(anyplan));
	}

	[HttpPut("planprices/update/{id}")]
	public async Task<IActionResult> PlanPricesUpdateAsync([FromRoute] Guid id, [FromBody] PlanPricesMeta planprice)
	{
		if (planprice == null)
		{
			return BadRequest("Invalid category payload.");
		}
		planprice.id = id;
		return ApiOk(await _plansService.PlanPricesUpsertAsync(planprice));
	}

	[HttpPost("planprices/create")]
	public async Task<IActionResult> PlanPricesCreateAsync([FromBody] PlanPricesMeta planprice)
	{
		if (planprice == null)
		{
			return BadRequest("Invalid category payload.");
		}
		planprice.id = null;
		return ApiOk(await _plansService.PlanPricesUpsertAsync(planprice));
	}

	[HttpGet("planprices/{plan_id}")]
	public async Task<IActionResult> GetPlanPricesByPlanIdAsync([FromRoute] Guid plan_id, [FromQuery] bool active_only = false)
	{
		return ApiOk(await _plansService.GetPlanPricesByPlanIdAsync(plan_id, active_only));
	}
}
