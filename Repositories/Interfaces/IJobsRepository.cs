using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models.Jobs;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface IJobsRepository
    {
        Task<JobPostResponseDto> CreateJobPostAsync(JobPostRequestDto request);
        Task<IEnumerable<MyJobsResponseDto>> MyJobPostsGetAsync(MyJobsRequestDto myjobsReq);
        Task<IEnumerable<Job>> GetJobsByUserTradesAndLocation(tUserJobsListRequestDto tUserJobsReq);
        Task<IEnumerable<Job>> GetUserPurchasedJobsList(tUserJobsListRequestDto tUserJobsReq);
        Task<JobFullDetailsDto> GetJobDetailsByIdFor_tUser_Async(Guid job_id);
        Task<JobFullDetailsDto> GetJobDetailsByIdFor_User_Async(Guid job_id);
        Task<PurchaseJobResultDto> JobPurchaseCreateAsync(PurchaseJobRequestDto pjrRequest);
        Task<DisputePurchaseJob> DisputePurchaseJobUpsert(DisputePurchaseJob dpjRequest);
        Task<IEnumerable<InterestedTradersDto>> GetInterestedTradersByJobId(Guid job_id);
        Task<CommonResponseDto> UpdateJobStatusAsync(JobUpdateStatus statDto);
        Task UpdateJobEntityAsync(JobUpdateEntityDto jueDto);
        Task<JobContactDto> UpdateAnyContactAsync(JobContactDto jcDto);

        //// future-ready methods (you will need them later)
        //Task<IEnumerable<object>> GetJobsAsync(Guid user_id);
        //Task<object> GetJobByIdAsync(Guid job_id);

        //Task<bool> UpdateJobAsync(JobPostRequestDto request);
    }
}
