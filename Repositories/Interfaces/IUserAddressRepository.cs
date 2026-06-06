using TradePlatform.Api.DTOs;
using TradePlatform.Api.Models;

public interface IUserAddressRepository
{
    Task<Guid> CreateCustomerProfileAsync(RegisterDto reg_dto);
    Task<Guid> CreateTradeUserBusinessAsync(RegisterDto reg_dto);

    Task<IEnumerable<UserAddress>> GetByEntityAsync(Guid entityId);

    //Task<UserAddress?> GetPrimaryAsync(Guid entityId, int addressTypeId);
}