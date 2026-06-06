using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models.Jobs;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Jobs
{
    public class JobsService: IJobsService
    {

        private readonly IJobsRepository _jobsRepo;

        public JobsService(IJobsRepository jobsRepo)
        {
            _jobsRepo = jobsRepo;
        }
        public async Task<IEnumerable<MyJobsResponseDto>> MyJobPostsGetAsync(MyJobsRequestDto userJobsReq)
        {
            return await _jobsRepo.MyJobPostsGetAsync(userJobsReq);
        }
        public async Task<JobPostResponseDto> CreateJobPostAsync(JobPostRequestDto jobPostReq)
        {
            return await _jobsRepo.CreateJobPostAsync(jobPostReq);
        }
        public async Task<PurchaseJobResultDto> JobPurchaseCreateAsync(PurchaseJobRequestDto pjrRequest)
        {
            return await _jobsRepo.JobPurchaseCreateAsync(pjrRequest);
        }
        public async Task<DisputePurchaseJob> DisputePurchaseJobUpsert(DisputePurchaseJob dpjRequest)
        {
            return await _jobsRepo.DisputePurchaseJobUpsert(dpjRequest);
        }
        public async Task<IEnumerable<Job>> GetJobsByUserTradesAndLocation(tUserJobsListRequestDto tUserJobsReq)            
        {
            return await _jobsRepo.GetJobsByUserTradesAndLocation(tUserJobsReq);
            
        }
        public async Task<IEnumerable<Job>> GetUserPurchasedJobsList(tUserJobsListRequestDto tUserJobsReq)
        {
            return await _jobsRepo.GetUserPurchasedJobsList(tUserJobsReq);

        }        
        public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_tUser_Async(Guid job_id)
        {
            return await _jobsRepo.GetJobDetailsByIdFor_tUser_Async(job_id);
        }
        public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_User_Async(Guid job_id)
        {
            return await _jobsRepo.GetJobDetailsByIdFor_User_Async(job_id);
        }
       

        public async Task<IEnumerable<InterestedTradersDto>> GetInterestedTradersByJobId(Guid job_id)
        {
            return await _jobsRepo.GetInterestedTradersByJobId(job_id);
        }

        public async Task<CommonResponseDto>UpdateJobStatusAsync(JobUpdateStatus statDto)
        {
            return await _jobsRepo.UpdateJobStatusAsync(statDto);
        }
        public async Task UpdateJobEntityAsync(JobUpdateEntityDto jueDto)
        {
            await _jobsRepo.UpdateJobEntityAsync(jueDto);
        }
        public async Task<JobContactDto> UpdateAnyContactAsync(JobContactDto jcDto)
        {
            return await _jobsRepo.UpdateAnyContactAsync(jcDto);
        }
    }
}
