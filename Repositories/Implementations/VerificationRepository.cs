using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models.document;

namespace TradePlatform.Api.Repositories.Implementations;

public class VerificationRepository
{
	private readonly DapperContext _context;

	public VerificationRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<VerifiedDocument> InsertVerifiedDocument(VerifiedDocument doc)
	{
		using IDbConnection connection = _context.CreateOpenConnection();
		DynamicParameters parameters = new DynamicParameters();
		parameters.Add("@user_id", doc.user_id);
		parameters.Add("@document_type", doc.document_type);
		parameters.Add("@document_number", doc.document_number);
		parameters.Add("@surname", doc.surname);
		parameters.Add("@given_names", doc.given_names);
		parameters.Add("@nationality", doc.nationality);
		parameters.Add("@date_of_birth", doc.date_of_birth);
		parameters.Add("@expiry_date", doc.expiry_date);
		parameters.Add("@issue_date", doc.issue_date);
		parameters.Add("@address", doc.address);
		parameters.Add("@visa_type", doc.visa_type);
		parameters.Add("@is_valid", doc.is_valid);
		parameters.Add("@raw_text", doc.raw_text);
		parameters.Add("@auto_verified", doc.auto_verified);
		CommandType? commandType = CommandType.StoredProcedure;
		return await connection.QueryFirstOrDefaultAsync<VerifiedDocument>("usp_user_verified_document_upsert", parameters, null, null, commandType);
	}
}
