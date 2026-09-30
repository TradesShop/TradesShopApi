using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.DTOs.Files;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations;

public class FileRepository : IFileRepository
{
	private readonly DapperContext _context;

	public FileRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<(uFile file, uFilelink link)> InsertFileWithLinkAsync(FileUploadRequestDto fu_req)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			user_id = fu_req.user_id,
			file_name = fu_req.filename,
			file_url = fu_req.file_url,
			file_type = fu_req.content_type,
			size_kb = fu_req.size_kb,
			entity_type = fu_req.entity_type,
			entity_id = fu_req.entity_id,
			upload_type = fu_req.upload_type,
			work_stage = fu_req.work_stage,
			is_primary = fu_req.is_primary
		};
		CommandType? commandType = CommandType.StoredProcedure;
		dynamic result = await conn.QuerySingleAsync<object>("usp_files_insert", param, null, null, commandType);
		uFile uFile2 = new uFile();
		uFile2.id = result.file_id;
		uFile2.file_name = result.file_name;
		uFile2.file_url = result.file_url;
		uFile2.file_type = result.file_type;
		uFile2.size_kb = result.size_kb;
		uFile2.created_at = result.created_at;
		uFile file = uFile2;
		uFilelink uFilelink2 = new uFilelink();
		uFilelink2.id = result.link_id;
		uFilelink2.file_id = result.file_id;
		uFilelink2.entity_type = result.entity_type;
		uFilelink2.entity_id = result.entity_id;
		uFilelink2.upload_type = result.upload_type;
		uFilelink2.is_primary = result.is_primary;
		uFilelink2.is_verified = result.is_verified;
		uFilelink2.created_at = result.created_at;
		uFilelink link = uFilelink2;
		return (file: file, link: link);
	}

	public async Task<IEnumerable<UploadFilesDto>> GetUploadFilesAsync(FilesGetRequestDto fgrDto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new { fgrDto.entity_id, fgrDto.entity_type, fgrDto.upload_type };
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryAsync<UploadFilesDto>("usp_files_get", param, null, null, commandType);
	}

	public async Task DeleteFileAsync(Guid fileId)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			file_id = fileId
		};
		CommandType? commandType = CommandType.StoredProcedure;
		await conn.ExecuteAsync("usp_files_delete", param, null, null, commandType);
	}

	public async Task UpdateDescriptionAsync(Guid fileId, string description)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@file_id", fileId);
		parameters.Add("@description", description);
		CommandType? commandType = CommandType.StoredProcedure;
		await connection.ExecuteAsync("usp_files_update_description", parameters, null, null, commandType);
	}
}
