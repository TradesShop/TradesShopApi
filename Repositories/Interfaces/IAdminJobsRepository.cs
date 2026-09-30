using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Admin;
using TradePlatform.Api.DTOs.Jobs;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IAdminJobsRepository
{
	Task<JobPostsSearchResult> SearchJobsAsync(JobPostsSearchReq req);

	Task<JobPostsSearchResult> DisputedJobsAsync(JobPostsSearchReq req);

	Task<JobPostsSearchResult> DisputeRequestedJobsAsync(JobPostsSearchReq req);

	Task<JobPostsSearchResult> PurchasedJobsAsync(JobPostsSearchReq req);

	Task<JobPostsSearchResult> NotPurchasedJobsAsync(JobPostsSearchReq req);

	Task<JobFullDetailsDto> GetJobDetailsByIdFor_admin_Async(Guid job_id);

	Task UpdateJobAsync(JobUpdateDto jueDto);
}
