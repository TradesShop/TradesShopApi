using System;

namespace TradePlatform.Api.Models;

public class AddressRemoveReq
{
	public Guid? user_id { get; set; }

	public int? address_id { get; set; }

	public Guid? business_id { get; set; }
}
