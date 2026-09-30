using TradePlatform.Api.Data;

namespace TradePlatform.Api.Repositories.Implementations;

public class CustomersRepository
{
	private readonly DapperContext _context;

	public CustomersRepository(DapperContext context)
	{
		_context = context;
	}
}
