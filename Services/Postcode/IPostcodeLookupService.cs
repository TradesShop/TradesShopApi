using System.Threading;
using System.Threading.Tasks;
using TradePlatform.Api.Models.Postcode;

namespace TradePlatform.Api.Services.Postcode;

public interface IPostcodeLookupService
{
	Task<PostcodeLookupResponse> LookupAsync(string postcode, string countryCode = "GB", CancellationToken cancellationToken = default(CancellationToken));

	Task<(double? Latitude, double? Longitude)> GeocodeWithOpenCageAsync(string query, CancellationToken cancellationToken);
}
