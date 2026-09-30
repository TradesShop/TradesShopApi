using System.Threading.Tasks;
using TradePlatform.Api.DTOs.SupportTicket;

namespace TradePlatform.Api.Repositories.PublicApi;

public interface IPublicApiRepository
{
	Task<RespopnseSupportTicket> CreateSupportTicket(CreateSupportTicketDto tckdto);
}
