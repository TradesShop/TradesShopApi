using System.Data;
using System.Threading.Tasks;
using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.DTOs.SupportTicket;

namespace TradePlatform.Api.Repositories.PublicApi;

public class PublicApiRepository : IPublicApiRepository
{
	private readonly DapperContext _context;

	public PublicApiRepository(DapperContext context)
	{
		_context = context;
	}

	public async Task<RespopnseSupportTicket> CreateSupportTicket(CreateSupportTicketDto tckdto)
	{
		using IDbConnection conn = _context.CreateOpenConnection();
		var param = new
		{
			tckdto.user_id, tckdto.user_type, tckdto.user_kind, tckdto.email, tckdto.full_name, tckdto.phone_number, tckdto.country_code, tckdto.category_id, tckdto.subject, tckdto.message_body,
			tckdto.ip_address
		};
		CommandType? commandType = CommandType.StoredProcedure;
		return await conn.QueryFirstOrDefaultAsync<RespopnseSupportTicket>("[dbo].[usp_support_ticket_create]", param, null, null, commandType);
	}
}
