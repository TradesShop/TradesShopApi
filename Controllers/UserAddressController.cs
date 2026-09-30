using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserAddressController : ControllerBase
{
	private readonly IUserAddressRepository _repo;

	public UserAddressController(IUserAddressRepository repo)
	{
		_repo = repo;
	}

	[HttpPost("upsert")]
	public async Task<IActionResult> Upsert([FromBody] UserAddress model)
	{
		if (model == null)
		{
			return BadRequest("Invalid address payload");
		}
		return Ok();
	}

	[HttpGet("entity/{entityid:long}")]
	public async Task<IActionResult> GetByEntity(Guid entityid)
	{
		return Ok(await _repo.GetByEntityAsync(entityid));
	}
}
