using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAddressController : ControllerBase
    {
        private readonly IUserAddressRepository _repo;

        public UserAddressController(IUserAddressRepository repo)
        {
            _repo = repo;
        }

        //// ⭐ POST: api/user-addresses/upsert
        //[HttpPost("upsert")]
        //public async Task<IActionResult> Upsert([FromBody] UserAddress model)
        //{
        //    if (model == null)
        //        return BadRequest("Invalid address payload");

        //    var result = await _repo.CreateAsync(model);
        //    return Ok(result);
        //}
        [HttpGet("entity/{entityid:long}")]
        public async Task<IActionResult> GetByEntity(Guid entityid)
        {
            var addresses = await _repo.GetByEntityAsync(entityid);
            return Ok(addresses);
        }

        // ⭐ GET: api/user-addresses/user/{userId}
        //[HttpGet("user/{userId:guid}")]
        //public async Task<IActionResult> GetByUser(Guid userId)
        //{
        //    var addresses = await _repo.GetByUserAsync(userId);
        //    return Ok(addresses);
        //}

        // ⭐ GET: api/user-addresses/user/{userId}/primary/{addressTypeId}
        //[HttpGet("user/{userId:guid}/primary/{addressTypeId:int}")]
        //public async Task<IActionResult> GetPrimary(Guid userId, int addressTypeId)
        //{
        //    var address = await _repo.GetPrimaryAsync(userId, addressTypeId);
        //    return Ok(address);
        //}
    }
}
