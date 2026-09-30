using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Admin;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Helpers;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations;

public class AdminJobsRepository : IAdminJobsRepository
{
	private readonly DapperContext _context;

	private readonly IIdentityService _identity;

	public AdminJobsRepository(DapperContext context, IIdentityService identity)
	{
		_context = context;
		_identity = identity;
	}

	public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_admin_Async(Guid job_id)
	{
		var (user_id, user_type) = _identity.GetIdentity();
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			id = job_id,
			user_id = user_id,
			user_type = user_type
		};
		CommandType? commandType = CommandType.StoredProcedure;
		using SqlMapper.GridReader multi = await conn.QueryMultipleAsync("[dbo].[usp_job_posts_get_details_by_id]", param, null, null, commandType);
		JobFullDetailsDto job = await multi.ReadFirstOrDefaultAsync<JobFullDetailsDto>();
		if (job == null)
		{
			return null;
		}
		job.Questions = JobQuestions.MapQuestions(await multi.ReadAsync<QuestionAnswerRow>());
		return job;
	}

	public async Task UpdateJobAsync(JobUpdateDto jueDto)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		var param = new
		{
			job_id = jueDto.job_id,
			user_id = _identity.GetUserId(),
			action = jueDto.action,
			status_id = jueDto.status_id,
			title = jueDto.title,
			description = jueDto.description,
			budget_range_id = jueDto.budget_range_id,
			timeline_id = jueDto.timeline_id,
			credit_cost = jueDto.credit_cost,
			purchase_limit = jueDto.purchase_limit
		};
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.QueryAsync("[dbo].[usp_admin_job_post_update]", param, null, null, commandType);
	}

	public async Task<JobPostsSearchResult> SearchJobsAsync(JobPostsSearchReq req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { req.search_text, req.postcode, req.category_id, req.status_id, req.search_from, req.search_to, req.page_number, req.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader anyresult = await conn.QueryMultipleAsync("[dbo].[usp_admin_job_posts_search]", param, null, null, commandType);
		return new JobPostsSearchResult
		{
			job_posts = anyresult.Read<JobPostsDto>().ToList(),
			total_records = anyresult.Read<int>().FirstOrDefault()
		};
	}

	public async Task<JobPostsSearchResult> DisputedJobsAsync(JobPostsSearchReq req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { req.search_text, req.postcode, req.category_id, req.status_id, req.search_from, req.search_to, req.target_user_id, req.page_number, req.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader anyresult = await conn.QueryMultipleAsync("[dbo].[usp_admin_jobs_disputed_search]", param, null, null, commandType);
		return new JobPostsSearchResult
		{
			job_posts = anyresult.Read<JobPostsDto>().ToList(),
			total_records = anyresult.Read<int>().FirstOrDefault()
		};
	}

	public async Task<JobPostsSearchResult> DisputeRequestedJobsAsync(JobPostsSearchReq req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { req.search_text, req.postcode, req.category_id, req.search_from, req.search_to, req.target_user_id, req.page_number, req.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader anyresult = await conn.QueryMultipleAsync("[dbo].[usp_admin_jobs_dispute_requested_search]", param, null, null, commandType);
		return new JobPostsSearchResult
		{
			job_posts = anyresult.Read<JobPostsDto>().ToList(),
			total_records = anyresult.Read<int>().FirstOrDefault()
		};
	}

	public async Task<JobPostsSearchResult> PurchasedJobsAsync(JobPostsSearchReq req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { req.search_text, req.postcode, req.category_id, req.search_from, req.search_to, req.target_user_id, req.page_number, req.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader anyresult = await conn.QueryMultipleAsync("[dbo].[usp_admin_jobs_purchased_search]", param, null, null, commandType);
		return new JobPostsSearchResult
		{
			job_posts = anyresult.Read<JobPostsDto>().ToList(),
			total_records = anyresult.Read<int>().FirstOrDefault()
		};
	}

	public async Task<JobPostsSearchResult> NotPurchasedJobsAsync(JobPostsSearchReq req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { req.search_text, req.postcode, req.category_id, req.search_from, req.search_to, req.page_number, req.page_size };
		CommandType? commandType = CommandType.StoredProcedure;
		SqlMapper.GridReader anyresult = await conn.QueryMultipleAsync("[dbo].[usp_admin_jobs_not_purchased_search]", param, null, null, commandType);
		return new JobPostsSearchResult
		{
			job_posts = anyresult.Read<JobPostsDto>().ToList(),
			total_records = anyresult.Read<int>().FirstOrDefault()
		};
	}
}
