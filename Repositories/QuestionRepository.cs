

using Dapper;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.Jobs;
using TradePlatform.Api.DTOs.Questions;
using TradePlatform.Api.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace TradePlatform.Api.Repositories
{
    public class QuestionRepository
    {
        private readonly DapperContext _context;

        public QuestionRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<QuestionDto?> GetQuestionsByCategory(int category_id)
        {
            using var connection = _context.CreateOpenConnection();

            var questionDict = new Dictionary<int, QuestionDto>();

            var result = await connection.QueryAsync<QuestionDto, AnswerQeDto, QuestionDto>(
                "usp_QuestionsByCategoryGetAsync",
                (q, a) =>
                {
                    if (!questionDict.TryGetValue(q.id, out var question))
                    {
                        question = q;
                        question.answers = new List<AnswerQeDto>();
                        questionDict.Add(question.id, question);
                    }

                    if (a != null && a.answerid != 0)
                    {
                        question.answers.Add(a);
                    }

                    return question;
                },
                new { category_id = category_id },
                commandType: CommandType.StoredProcedure,
                splitOn: "answerid"
            );
            return questionDict.Values.FirstOrDefault();
            //return questionDict.Values
            //    .OrderBy(x => x.sortorder)
            //    .ToList();
        }
        public async Task<int?> GetNextQuestionId(RequestForNextQue nQue)
        {

            var table = new DataTable();
            table.Columns.Add("id", typeof(int));

            foreach (var id in nQue.answer_ids)
                table.Rows.Add(id);
            using var connection = _context.CreateOpenConnection();

            return await connection.QueryFirstOrDefaultAsync<int?>(
                "usp_QuestionNextIdGetAsync",
                new { 
                    question_id=nQue.question_id,
                    answer_ids = table.AsTableValuedParameter("dbo.AnswerIdsList") 
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<QuestionDto?> GetQuestionWithAnswers(int question_id)
        {
            using var connection = _context.CreateConnection();

            var questionDict = new Dictionary<int, QuestionDto>();

            await connection.QueryAsync<QuestionDto, AnswerQeDto, QuestionDto>(
                "usp_QuestionWithAnswersGetAsync",
                (q, a) =>
                {
                    if (!questionDict.TryGetValue(q.id, out var question))
                    {
                        question = q;
                        question.answers = new List<AnswerQeDto>();
                        questionDict.Add(question.id, question);
                    }

                    if (a != null && a.answerid != 0)
                    {
                        question.answers.Add(a);
                    }

                    return question;
                },
                new { question_id = question_id },
                commandType: CommandType.StoredProcedure,
                splitOn: "answerid"
            );

            return questionDict.Values.FirstOrDefault();
        }

        public async Task<List<QuestionDto>> GetQuestionsForPostJob(Guid job_id)
        {
            using var connection = _context.CreateOpenConnection();

            var questionDict = new Dictionary<int, QuestionDto>();

            await connection.QueryAsync<QuestionDto, AnswerQeDto, QuestionDto>(
                "usp_job_get_questions_for_postjob",
                (q, a) =>
                {
                    if (!questionDict.TryGetValue(q.id, out var question))
                    {
                        question = q;
                        question.answers = new List<AnswerQeDto>();
                        questionDict.Add(question.id, question);
                    }

                    if (a != null && a.answerid > 0)
                    {
                        question.answers.Add(a);
                    }

                    return question;
                },
                new { job_id = job_id },   // ✔ correct parameter name
                commandType: CommandType.StoredProcedure,
                splitOn: "answerid"
            );

            return questionDict.Values.ToList();   // ✔ return full list
        }

        public async Task UpsertAnswerAsync(AnswerUpsertDto auDto)
        {
            using var connection = _context.CreateOpenConnection();
            await connection.ExecuteAsync(
                "usp_job_post_answer_upsert",
                new
                {
                    job_id = auDto.job_id,
                    question_id = auDto.question_id,
                    answer_id = auDto.answer_id
                },
                commandType: CommandType.StoredProcedure
            );
        }

    }
}

     

    //public async Task<QuestionDto> GetQuestion(int questionid)
    //    {
    //        using var conn = _context.CreateConnection();

    //        var questionDictionary = new Dictionary<int, QuestionDto>();

    //        var result = await conn.QueryAsync<QuestionDto, AnswerDto, QuestionDto>(
    //            "dbo.GetQuestionWithAnswers",
    //            (q, a) =>
    //            {
    //                if (!questionDictionary.TryGetValue(q.id, out var question))
    //                {
    //                    question = new QuestionDto
    //                    {
    //                        id = q.id,
    //                        title = q.title,
    //                        type = q.type,
    //                        answers = new List<AnswerDto>()
    //                    };

    //                    questionDictionary.Add(question.id, question);
    //                }

    //                if (a != null && a.answerid != 0)
    //                {
    //                    question.answers.Add(new AnswerDto
    //                    {
    //                        id = a.answerid,
    //                        uxtext = a.answertext,
    //                        additional_queid = a.additional_queid
    //                    });
    //                }

    //                return question;
    //            },
    //            param: new { question_id = questionid },
    //            commandType: CommandType.StoredProcedure,
    //            splitOn: "answerid"
    //        );

    //        return questionDictionary.Values.FirstOrDefault();
    //    }
    //}

