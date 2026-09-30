using System.Threading.Tasks;
using TradePlatform.Api.DTOs.SupportTicket;

namespace TradePlatform.Api.Services.PublicApi;

public interface IPublicApiService
{
	Task<RespopnseSupportTicket> CreateSupportTicket(CreateSupportTicketDto tckdto);
}
