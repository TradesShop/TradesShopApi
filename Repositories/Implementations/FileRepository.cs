using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class FileRepository: IFileRepository
    {
        private readonly DapperContext _context;

        public FileRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<(uFile file, uFilelink link)> InsertFileWithLinkAsync(
            string fileName,
            string fileUrl,
            string fileType,
            int sizeKb,
            int entityType,
            Guid entityId,
            string uploadType,
            string workStage,
            bool isPrimary)
        {
            using var conn = _context.CreateOpenConnection();

            var result = await conn.QuerySingleAsync<dynamic>(
                "usp_files_insert",
                new
                {
                    file_name = fileName,
                    file_url = fileUrl,
                    file_type = fileType,
                    size_kb = sizeKb,
                    entity_type = entityType,
                    entity_id = entityId,
                    upload_type = uploadType,
                    work_stage=workStage,
                    is_primary = isPrimary
                },
                commandType: CommandType.StoredProcedure
            );

            var file = new uFile
            {
                id = result.file_id,
                file_name = result.file_name,
                file_url = result.file_url,
                file_type = result.file_type,
                size_kb = result.size_kb,
                created_at = result.created_at
            };

            var link = new uFilelink
            {
                id = result.link_id,
                file_id = result.file_id,
                entity_type = result.entity_type,
                entity_id = result.entity_id,
                upload_type = result.upload_type,
                is_primary = result.is_primary,
                is_verified = result.is_verified,
                created_at = result.created_at
            };

            return (file, link);
        }

        public async Task<IEnumerable<UploadFilesDto>> GetUploadFilesAsync(FilesGetRequestDto fgrDto)
        {
            using var conn = _context.CreateOpenConnection();

            return await conn.QueryAsync<UploadFilesDto>(
                "usp_files_get",
                new
                {
                    entity_id = fgrDto.entity_id,
                    entity_type = fgrDto.entity_type,
                    upload_type = fgrDto.upload_type
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task DeleteFileAsync(Guid fileId)
        {
            using var conn = _context.CreateOpenConnection();

            await conn.ExecuteAsync(
                "usp_files_delete",
                new { file_id = fileId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task UpdateDescriptionAsync(Guid fileId, string description)
        {
            using var connection = _context.CreateOpenConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@file_id", fileId);
            parameters.Add("@description", description);

            await connection.ExecuteAsync(
                "usp_files_update_description",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }


    }
}
