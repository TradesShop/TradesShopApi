using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ICustomersRepository
    {
        Task CreateAsync(UserAddress customer);
    }
}
