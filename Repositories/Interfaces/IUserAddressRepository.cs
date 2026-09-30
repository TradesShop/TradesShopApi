using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.Models;

public interface IUserAddressRepository
{
	Task<Guid> CreateCustomerProfileAsync(RegisterDto reg_dto);

	Task<Guid> CreateTradeUserBusinessAsync(RegisterDto reg_dto);

	Task<IEnumerable<UserAddress>> GetByEntityAsync(Guid entityId);
}
