using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradePlatform.Api.Models.Postcode;
using TradePlatform.Api.Services.Postcode;

namespace TradePlatform.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostcodeController : BaseController
{
	private readonly IPostcodeLookupService _postcodeService;

	public PostcodeController(IPostcodeLookupService postcodeService)
	{
		_postcodeService = postcodeService;
	}

	[HttpGet("lookup")]
	[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "postcode", "country" })]
	[ProducesResponseType(typeof(PostcodeLookupResponse), 200)]
	[ProducesResponseType(typeof(PostcodeLookupResponse), 400)]
	[ProducesResponseType(typeof(PostcodeLookupResponse), 404)]
	[ProducesResponseType(typeof(PostcodeLookupResponse), 502)]
	public async Task<IActionResult> Lookup([FromQuery][Required] string postcode, [FromQuery] string country = "GB", CancellationToken cancellationToken = default(CancellationToken))
	{
		PostcodeLookupResponse result = await _postcodeService.LookupAsync(postcode, country, cancellationToken);
		if (result.success)
		{
			return ApiOk(result.data, "Postcode lookup successful");
		}
		return result.error switch
		{
			"Postcode is required" => ApiError(null, result.error), 
			"Unsupported country code" => ApiError(null, result.error), 
			"Invalid postcode" => ApiError(null, result.error, 404), 
			"No results found for this postcode" => ApiError(null, result.error, 404), 
			_ => ApiError(null, result.error, 502), 
		};
	}

	[HttpGet("office/geocode")]
	public async Task<IActionResult> GeocodeOffice(string office, string district, string state, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(office))
		{
			return ApiError("Office name is required");
		}
		string query = $"{office}, {district}, {state}, India";
		var (lat, lon) = await _postcodeService.GeocodeWithOpenCageAsync(query, cancellationToken);
		if (!lat.HasValue || !lon.HasValue)
		{
			return ApiError("Unable to geocode office location");
		}
		return ApiOk(new
		{
			name = office,
			latitude = lat,
			longitude = lon
		});
	}
}
