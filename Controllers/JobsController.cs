using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.users;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Jobs;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class JobsController : BaseController
    {
        
        private readonly IJobsService _jobsService;
       
        public JobsController(            
             IJobsService jobsService,
             
        IHttpContextAccessor http
        ) : base(http)
        {            
           _jobsService= jobsService;
           
        }
        
        
        [HttpPost("post")]
        public async Task<IActionResult> CreateAJobPostAsync([FromBody] JobPostRequestDto jPostDtos)
        {
            var anyresult = await _jobsService.CreateJobPostAsync(jPostDtos);
            return ApiOk(anyresult);

        }
        [HttpPost("myjobs")]
        public async Task<IActionResult> myjobs([FromBody] MyJobsRequestDto myjobsDtos )
        {
            var (user_id, callerType) = GetIdentity();
            myjobsDtos.user_id = user_id;
            var anyresult = await _jobsService.MyJobPostsGetAsync(myjobsDtos);
            return ApiOk(anyresult);

        }
        [HttpPost("list")]
        public async Task<IActionResult> GetRecommendedJobs([FromBody] tUserJobsListRequestDto tUserJobsReq) {
            var (callerId, callerType) = GetIdentity();
            tUserJobsReq.user_id = ResolveEffectiveUser(
                callerId,
                callerType,
                tUserJobsReq?.target_user_id
            );

            var jobs = await _jobsService.GetJobsByUserTradesAndLocation(tUserJobsReq);               

            return ApiOk(jobs);
        }
        [HttpPost("purchased")]
        public async Task<IActionResult> GetUserPurchasedJobs([FromBody] tUserJobsListRequestDto tUserJobsReq)
        {
            var (callerId, callerType) = GetIdentity();
            tUserJobsReq.user_id = ResolveEffectiveUser(
                callerId,
                callerType,
                tUserJobsReq?.target_user_id
            );

            var jobs = await _jobsService.GetUserPurchasedJobsList(tUserJobsReq);

            return ApiOk(jobs);
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseJob([FromBody] PurchaseJobRequestDto pjrRequest)
        {
            var (user_id, callerType) = GetIdentity();
            pjrRequest.user_id = user_id;
            var result = await _jobsService.JobPurchaseCreateAsync(pjrRequest);

            if (!result.success)
                return ApiError(result);

            var jobDetails = await _jobsService.GetJobDetailsByIdFor_tUser_Async(pjrRequest.id);
            return ApiOk(jobDetails);
        }
        [HttpPost("dispute")]
        public async Task<IActionResult> DisputePurchaseJob([FromBody] DisputePurchaseJob dpjRequest)
        {
            var (user_id, callerType) = GetIdentity();
            dpjRequest.raised_by_user_id = user_id;
            dpjRequest.status_id = 21;/*pending*/
            var result = await _jobsService.DisputePurchaseJobUpsert(dpjRequest);
            //if (!result.success)
            //return ApiError(result);
            return ApiOk(result);
        }
        //[HttpPost("dispute/upsert")]
        //public async Task<IActionResult> DisputePurchaseJobUpsert([FromBody] DisputePurchaseJob dpjRequest)
        //{
        //    var (user_id, callerType) = GetIdentity();
        //    dpjRequest.raised_by_user_id = user_id;
        //    var result = await _jobsService.DisputePurchaseJobUpsert(dpjRequest);
        //    //if (!result.success)
        //        //return ApiError(result);
        //    return ApiOk(result);
        //}
        [HttpGet("{job_id}")]
        public async Task<IActionResult> GetJobDetailsByIdFor_tUser_Async(Guid job_id)
        {
            var anyresult = await _jobsService.GetJobDetailsByIdFor_tUser_Async(job_id);
            return ApiOk(anyresult);
        }
        [HttpGet("myjobs/{job_id}")]
        public async Task<IActionResult> GetJobDetailsByIdFor_User_Async(Guid job_id)
        {           
            var anyresult = await _jobsService.GetJobDetailsByIdFor_User_Async(job_id);
            return ApiOk(anyresult);
        }
        [HttpGet("interested_trades/{job_id}")]
        public async Task<IActionResult> GetInterestedTradersByJobId(Guid job_id)
        {
            var anyresults = await _jobsService.GetInterestedTradersByJobId(job_id);
            //foreach (var item in anyresults)
            //{
            //    if (!string.IsNullOrEmpty(item.logoUrl))
            //    {
            //        item.logoUrl = _blob.GetReadSasUrl(item.logoUrl);
            //    }
            //}           

            return ApiOk(anyresults);
        }

        [HttpPost("update/status")]
        public async Task<IActionResult> UpdateJobStatusAsync([FromBody] JobUpdateStatus statDto)
        {
            var anyresult=await _jobsService.UpdateJobStatusAsync(statDto);

            return ApiOk(anyresult);
        }
        [HttpPost("update/entity")]
        public async Task<IActionResult> UpdateJobEntityAsync([FromBody] JobUpdateEntityDto jueDto)
        {
            await _jobsService.UpdateJobEntityAsync(jueDto);
            return ApiOk(new { success = true});
        }
        [HttpPost("contact/upsert")]
        public async Task<IActionResult> UpdateAnyContactAsync(JobContactDto jcDto)
        {            
            var (user_id, callerType) = GetIdentity();
            jcDto.user_id = user_id;
            var anyresult = await _jobsService.UpdateAnyContactAsync(jcDto);
            if (anyresult == null)
                return ApiError(anyresult);
            return ApiOk(anyresult);

        }

    }
}
