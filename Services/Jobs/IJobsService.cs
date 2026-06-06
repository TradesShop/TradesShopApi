using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models.Jobs;

namespace TradePlatform.Api.Services.Jobs
{
    public interface IJobsService
    {
        Task<IEnumerable<MyJobsResponseDto>> MyJobPostsGetAsync(MyJobsRequestDto userJobsReq);
        Task<JobPostResponseDto> CreateJobPostAsync(JobPostRequestDto jobPostReq);
        Task<PurchaseJobResultDto> JobPurchaseCreateAsync(PurchaseJobRequestDto pjrRequest);
        Task<DisputePurchaseJob> DisputePurchaseJobUpsert(DisputePurchaseJob dpjRequest);
        Task<IEnumerable<Job>> GetJobsByUserTradesAndLocation(tUserJobsListRequestDto tUserJobsReq);
        Task<IEnumerable<Job>> GetUserPurchasedJobsList(tUserJobsListRequestDto tUserJobsReq);
        Task<JobFullDetailsDto> GetJobDetailsByIdFor_tUser_Async(Guid job_id);
        Task<JobFullDetailsDto> GetJobDetailsByIdFor_User_Async(Guid job_id);
        Task<IEnumerable<InterestedTradersDto>> GetInterestedTradersByJobId(Guid job_id);
       
        Task<CommonResponseDto> UpdateJobStatusAsync(JobUpdateStatus statDto);
        Task UpdateJobEntityAsync(JobUpdateEntityDto jueDto);
        Task<JobContactDto> UpdateAnyContactAsync(JobContactDto jcDto);
    }

}
