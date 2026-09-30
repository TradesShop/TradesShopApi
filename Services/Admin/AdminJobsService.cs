using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Admin;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Admin;

public class AdminJobsService : IAdminJobsService
{
	private readonly IAdminJobsRepository _adminJobRepo;

	public AdminJobsService(IAdminJobsRepository adminJobRepo)
	{
		_adminJobRepo = adminJobRepo;
	}

	public Task<JobPostsSearchResult> SearchJobsAsync(JobPostsSearchReq req)
	{
		return _adminJobRepo.SearchJobsAsync(req);
	}

	public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_admin_Async(Guid job_id)
	{
		return await _adminJobRepo.GetJobDetailsByIdFor_admin_Async(job_id);
	}

	public async Task UpdateJobAsync(JobUpdateDto jueDto)
	{
		await _adminJobRepo.UpdateJobAsync(jueDto);
	}

	public Task<JobPostsSearchResult> DisputedJobsAsync(JobPostsSearchReq req)
	{
		return _adminJobRepo.DisputedJobsAsync(req);
	}

	public Task<JobPostsSearchResult> DisputeRequestedJobsAsync(JobPostsSearchReq req)
	{
		return _adminJobRepo.DisputeRequestedJobsAsync(req);
	}

	public Task<JobPostsSearchResult> PurchasedJobsAsync(JobPostsSearchReq req)
	{
		return _adminJobRepo.PurchasedJobsAsync(req);
	}

	public Task<JobPostsSearchResult> NotPurchasedJobsAsync(JobPostsSearchReq req)
	{
		return _adminJobRepo.NotPurchasedJobsAsync(req);
	}
}
