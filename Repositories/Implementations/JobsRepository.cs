using Dapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Jobs;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class JobsRepository : IJobsRepository
    {
        private readonly DapperContext _context;
        private readonly IIdentityService _identity;

        public JobsRepository(DapperContext context, IIdentityService identity)
        {
            _context = context;
            _identity = identity;
        }
        private string mask_phone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            return phone.Substring(0, 3) + "********";
        }

        private string mask_email(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];

            return name.Substring(0, 2) + "****@" + domain.Substring(0, 1) + "****";
        }

        private string mask_customer(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            return name.Substring(0, 2) + "****";
        }
        // ---------------------------------------------------------
        // CREATE JOB POST
        // ---------------------------------------------------------
        public async Task<JobPostResponseDto> CreateJobPostAsync(JobPostRequestDto request)
        {
            using var conn = _context.CreateOpenConnection();

            var table = new DataTable();
            table.Columns.Add("question_id", typeof(int));
            table.Columns.Add("answer_id", typeof(int));
            table.Columns.Add("answer_text", typeof(string));

            foreach (var item in request.answers)
            {
                if (item.answer_ids is IEnumerable<int> list)
                {
                    foreach (var a in list)
                    {
                        table.Rows.Add(item.question_id, a, DBNull.Value);
                    }
                }
                else if (item.answer_id is int single)
                {
                    table.Rows.Add(item.question_id, single, DBNull.Value);
                }
            }
            var workplacejson = JsonConvert.SerializeObject(request.workplace);

            var parameters = new
            {
                user_id = _identity.GetUserId(),
                category_id = request.category_id,
                firstname=request.firstname,
                lastname = request.lastname,
                phone=request.phone,
                email=request.email,
                title = request.title,
                description = request.description,
                timeline_id = request.timeline_id,                
                budget_range_id=request.budget_range_id,                
                postcode = request.postcode,
                latitude = request.latitude,
                longitude = request.longitude,
                created_by = _identity.GetUserId(),                
                ip_address = _identity.GetIpAddress(),
                user_agent = _identity.GetUserAgent(),
                workplace = workplacejson,
                answers = table.AsTableValuedParameter("JobPostAnswerType")
                
        };

            var result = await conn.QueryFirstOrDefaultAsync<JobPostResponseDto>(
                "usp_job_posts_create_async",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        public async Task<IEnumerable<Job>> GetJobsByUserTradesAndLocation(tUserJobsListRequestDto tUserJobsReq)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@user_id", tUserJobsReq.user_id);
            parameters.Add("@status_id", tUserJobsReq.status_id);
            parameters.Add("@last_created_at", tUserJobsReq.last_created_at);
            parameters.Add("@last_id", tUserJobsReq.last_id);
            parameters.Add("@limit", tUserJobsReq.limit);
            parameters.Add("@sort_key", tUserJobsReq.sort_key);
            parameters.Add("@sort_dir", tUserJobsReq.sort_dir);
            var result = await connection.QueryAsync<Job>(
                "usp_jobs_get_by_user_trades_and_location",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        public async Task<IEnumerable<Job>> GetUserPurchasedJobsList(tUserJobsListRequestDto tUserJobsReq)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@user_id", tUserJobsReq.user_id);            
            parameters.Add("@last_created_at", tUserJobsReq.last_created_at);
            parameters.Add("@last_id", tUserJobsReq.last_id);
            parameters.Add("@limit", tUserJobsReq.limit);
            parameters.Add("@sort_key", tUserJobsReq.sort_key);
            parameters.Add("@sort_dir", tUserJobsReq.sort_dir);
            var result = await connection.QueryAsync<Job>(
                "usp_jobs_get_purchased_list",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        
        public async Task<IEnumerable<MyJobsResponseDto>> MyJobPostsGetAsync(MyJobsRequestDto myjobsReq)
        {
            using var conn = _context.CreateOpenConnection();
          
            var parameters = new DynamicParameters();
            parameters.Add("@user_id", myjobsReq.user_id);
            parameters.Add("@status_id", myjobsReq.status_id);
            parameters.Add("@last_created_at", myjobsReq.last_created_at);
            parameters.Add("@last_id", myjobsReq.last_id);
            parameters.Add("@limit", myjobsReq.limit);
            var anyresult = await conn.QueryAsync<MyJobsResponseDto>(
                "usp_jobs_mylist_get_async",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return anyresult;
        }
        private List<JobQuestionDto> MapQuestions(IEnumerable<QuestionAnswerRow?> qaRows)
        {
            return qaRows
                .GroupBy(r => new { r.question_id, r.question_title, r.que_group_id })
                .Select(g => new JobQuestionDto
                {
                    question_id = g.Key.question_id,
                    question_title = g.Key.question_title,
                    que_group_id = g.Key.que_group_id,

                    answers = g.Select(a => new AnswerDto
                    {
                        answer_id = a.answer_id,
                        answer_title = a.answer_title
                    }).ToList(),

                    answers_csv = string.Join(", ", g.Select(a => a.answer_title))
                })
                .ToList();
        }
           /*This method for return job details who submitted */
        public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_User_Async(Guid job_id)
        {
            using var conn = _context.CreateOpenConnection();
            using var multi = await conn.QueryMultipleAsync(
                "usp_job_get_by_id_for_user",
                new { id = job_id, user_id = _identity.GetUserId() },
                commandType: CommandType.StoredProcedure
            );
            // 1. Job details
            var job = await multi.ReadFirstOrDefaultAsync<JobFullDetailsDto>();
            if (job == null)
                return null;           
            // 2. Flat Q/A rows
            var qaRows = await multi.ReadAsync<QuestionAnswerRow>();
            // 3. Group answers under each question
            job.Questions = MapQuestions(qaRows);
            return job;
        }
        public async Task<JobFullDetailsDto> GetJobDetailsByIdFor_tUser_Async(Guid job_id)
        {
            using var conn = _context.CreateOpenConnection();
            using var multi = await conn.QueryMultipleAsync(
                "usp_job_get_by_id_for_tuser",
                new { id = job_id,user_id= _identity.GetUserId() },
                commandType: CommandType.StoredProcedure
            );
            // 1. Job details
            var job = await multi.ReadFirstOrDefaultAsync<JobFullDetailsDto>();
            if (job == null)
                return null;
            if (!job.job_purchase_id.HasValue || job.job_purchase_id == Guid.Empty)
            {
                //job.customer_name = mask_customer(job.customer_name);
                job.phone = mask_phone(job.phone);
                job.email = mask_email(job.email);
            }
            // 2. Flat Q/A rows
            var qaRows = await multi.ReadAsync<QuestionAnswerRow>();
            // 3. Group answers under each question
            job.Questions = MapQuestions(qaRows); 
            return job;
        }

        public async Task<PurchaseJobResultDto> JobPurchaseCreateAsync(PurchaseJobRequestDto pjrRequest)
        {
            using var conn = _context.CreateOpenConnection();

            var purchaseResult = await conn.QueryFirstOrDefaultAsync<PurchaseJobResultDto>(
                    "usp_job_purchase_create_async",
                    new
                    {
                        user_id= pjrRequest.user_id,
                        job_id= pjrRequest.id
                    },
                    commandType: CommandType.StoredProcedure
                );

            if (purchaseResult == null)
            {
                return new PurchaseJobResultDto
                {
                    success = false,
                    message = "No response from database"
                };
            }
            return purchaseResult;
        }
        public async Task<DisputePurchaseJob> DisputePurchaseJobUpsert(DisputePurchaseJob dpjRequest)
        {
            using var conn = _context.CreateOpenConnection();

            var anydispute = await conn.QueryFirstOrDefaultAsync<DisputePurchaseJob>(
                    "usp_job_post_dispute_upsert",
                    new
                    {
                        id= dpjRequest.id,
                        job_purchase_id= dpjRequest.job_purchase_id,
                        job_id = dpjRequest.job_id,
                        raised_by_user_id = dpjRequest.raised_by_user_id,
                        against_user_id = dpjRequest.against_user_id,
                        dispute_type = dpjRequest.dispute_type,
                        reason = dpjRequest.reason,
                        status_id = dpjRequest.status_id,
                        resolution_notes = dpjRequest.resolution_notes,
                        refund_credits = dpjRequest.refund_credits,
                        decision = dpjRequest.decision
                    },
                    commandType: CommandType.StoredProcedure
                );

           
            return anydispute;
        }


        public async Task<IEnumerable<InterestedTradersDto>> GetInterestedTradersByJobId(Guid job_id)
        {
            using var connection = _context.CreateOpenConnection();

            var result = await connection.QueryAsync<InterestedTradersDto>(
                "usp_job_interested_trades_by_jobid",
                  new { job_id = job_id },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }      

        public async Task<CommonResponseDto> UpdateJobStatusAsync(JobUpdateStatus statDto)
        {
            using var connection = _context.CreateOpenConnection();
            var result = await connection.QueryFirstOrDefaultAsync<CommonResponseDto>(
               "usp_job_post_update_status",
                 new { 
                     job_id = statDto.job_id
                     ,statuscode= statDto.statuscode
                     ,user_id= _identity.GetUserId()
                     ,closure_reason_id=statDto.closure_reason_id
                     ,completed_by= statDto.completed_by
                     ,note = statDto.note
                     ,ip_address = _identity.GetIpAddress()
                     ,user_agent = _identity.GetUserAgent()
                 },
               commandType: CommandType.StoredProcedure
           );
            return result;
        }
        public async Task UpdateJobEntityAsync(JobUpdateEntityDto jueDto)
        {
            using var connection = _context.CreateOpenConnection();
            var result = await connection.QueryAsync(
               "usp_job_post_update_entity",
                 new
                 {
                     job_id = jueDto.job_id,
                     user_id = _identity.GetUserId(),
                     action= jueDto.action,
                     title = jueDto.title,
                     description = jueDto.description,
                     budget_range_id=jueDto.budget_range_id

                 },
               commandType: CommandType.StoredProcedure
           );
        }
        public async Task<JobContactDto> UpdateAnyContactAsync(JobContactDto jcDto)
        {
            using var connection = _context.CreateOpenConnection();
            var result = await connection.QueryFirstOrDefaultAsync<JobContactDto>(
               "usp_job_contact_upsert",
                 new
                 {
                     contact_id= jcDto.contact_id,
                     job_id = jcDto.job_id,
                     user_id= jcDto.user_id,
                     firstname = jcDto.firstname,
                     lastname = jcDto.lastname,
                     email = jcDto.email,
                     phone = jcDto.phone                    
                 },
               commandType: CommandType.StoredProcedure
           );
            return result;
        }
    }
}
