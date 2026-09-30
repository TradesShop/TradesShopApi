using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Models.Jobs;

namespace TradePlatform.Api.Repositories.Interfaces;

public interface IJobsRepository
{
	Task<JobPostResponseDto> CreateJobPostAsync(JobPostRequestDto request);

	Task<IEnumerable<MyJobsResponseDto>> MyJobPostsGetAsync(MyJobsRequestDto myjobsReq);

	Task<IEnumerable<Job>> GetJobsByUserTradesAndLocation(tUserJobsListRequestDto tUserJobsReq);

	Task<IEnumerable<Job>> GetUserPurchasedJobsList(tUserJobsListRequestDto tUserJobsReq);

	Task<JobFullDetailsDto> GetJobDetailsByIdFor_tUser_Async(Guid job_id);

	Task<JobFullDetailsDto> GetJobDetailsByIdFor_User_Async(Guid job_id);

	Task<PurchaseJobResultDto> JobPurchaseCreateAsync(PurchaseJobRequestDto pjrRequest);

	Task<DisputeJobResponse> DisputeJobGetByDisputeId(Guid dispute_id, Guid? user_id);

	Task<IEnumerable<InterestedTradersDto>> GetInterestedTradersByJobId(Guid job_id);

	Task<CommonResponseDto> UpdateJobStatusAsync(JobUpdateStatus statDto);

	Task UpdateJobEntityAsync(JobUpdateEntityDto jueDto);

	Task<JobContactDto> UpdateAnyContactAsync(JobContactDto jcDto);

	Task<JobLocationDto> UpdateJobLocationAsync(JobLocationDto jlDto);

	Task<NotifyEmailDataModel> JobPurchaseNotifyEmailDetails(PurchaseJobResultDto jp_dto);

	Task<IEnumerable<TraderDto>> JobPostNotifyEmailDetailsForTraders(Guid job_id);

	Task JobNotificationUpsertAsync(JobNotificationUpsertReq jnReq);

	Task<IEnumerable<DisputeTraders>> GetDisputeTradersByJobIdAsync(Guid job_id);

	Task<DisputeJobResponse> JobDisputeActionAsync(DisputeActionReq jdaDto);

	Task<Job_meta> AssignTradeAsync(JobAssignTradeRequest jat_req);

	Task<DisputeJobResponse> DisputePurchaseJobSubmit(DisputePurchaseJob dpjRequest);

	Task<DisputeJobResponse> DisputePurchaseJobUpsert(DisputePurchaseJob dpjRequest);

	Task<DisputeJobResponse> JobDisputeUpdateStatus(DisputeStatusRequest dsReq);
}
