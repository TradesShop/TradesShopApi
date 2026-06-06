using Dapper;
using TradePlatform.Api.Data;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Repositories.Implementations
{
    public class CustomersRepository 
    {
        private readonly DapperContext _context;

        public CustomersRepository(DapperContext context)
        {
            _context = context;
        }

        //public async Task CreateAsync(Addresses customer)
        //{
        //    customer.Id = Guid.NewGuid();
        //    customer.Created_At = DateTime.UtcNow;
        //    customer.Updated_At = DateTime.UtcNow;

        //    using var conn = _context.CreateConnection();

        //    await conn.ExecuteAsync(
        //        "spCustomers_Create",
        //        new
        //        {
        //            customer.Id,
        //            customer.User_Id,
        //            customer.Postcode,
        //            customer.Address,
        //            customer.Country_Id,
        //            customer.GLng,
        //            customer.GLat,
        //            customer.Created_At,
        //            customer.Updated_At
        //        },
        //        commandType: System.Data.CommandType.StoredProcedure
        //    );
        //}
    }
}
