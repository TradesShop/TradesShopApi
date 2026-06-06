using System.Threading.Tasks;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Repositories.Interfaces
{
    public interface ITradesRepository
    {
        Task<IEnumerable<Trades>> GetTradesAsync(int? id);
        //Task CreateAsync(Tradesperson tradesperson);
    }
}
