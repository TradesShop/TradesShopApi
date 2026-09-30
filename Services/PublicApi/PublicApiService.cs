using System.Threading.Tasks;
using TradePlatform.Api.DTOs.SupportTicket;
using TradePlatform.Api.Repositories.PublicApi;

namespace TradePlatform.Api.Services.PublicApi;

public class PublicApiService : IPublicApiService
{
	private readonly IPublicApiRepository _publicRepository;

	public PublicApiService(IPublicApiRepository publicRepository)
	{
		_publicRepository = publicRepository;
	}

	public async Task<RespopnseSupportTicket> CreateSupportTicket(CreateSupportTicketDto tckdto)
	{
		return await _publicRepository.CreateSupportTicket(tckdto);
	}
}
