using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradePlatform.Api.DTOs.OpenCage;
using TradePlatform.Api.Models.Postcode;

namespace TradePlatform.Api.Services.Postcode;

public sealed class PostcodeLookupService : IPostcodeLookupService
{
	private sealed class PostcodesIoResponse
	{
		[JsonPropertyName("result")]
		public PostcodesIoResult? Result { get; set; }
	}

	private sealed class PostcodesIoResult
	{
		public string? Postcode { get; set; }

		public string? AdminDistrict { get; set; }

		public string? Parish { get; set; }

		public string? AdminWard { get; set; }

		public string? Region { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }
	}

	private sealed class IndiaApiResponse
	{
		public string? Status { get; set; }

		public List<IndiaPostOffice>? PostOffice { get; set; }
	}

	private sealed class IndiaPostOffice
	{
		public string? Name { get; set; }

		public string? Block { get; set; }

		public string? District { get; set; }

		public string? State { get; set; }
	}

	private sealed class ZippopotamResponse
	{
		[JsonPropertyName("post code")]
		public string? PostCode { get; set; }

		public string? Country { get; set; }

		public List<ZippopotamPlace>? Places { get; set; }
	}

	private sealed class ZippopotamPlace
	{
		[JsonPropertyName("place name")]
		public string? PlaceName { get; set; }

		public string? State { get; set; }

		public string? County { get; set; }

		public string? Latitude { get; set; }

		public string? Longitude { get; set; }
	}

	private sealed class NominatimResult
	{
		public string? Lat { get; set; }

		public string? Lon { get; set; }

		public NominatimAddress? Address { get; set; }
	}

	private sealed class NominatimAddress
	{
		public string? Country { get; set; }

		public string? City { get; set; }

		public string? Town { get; set; }

		public string? Village { get; set; }

		public string? Hamlet { get; set; }

		public string? Locality { get; set; }

		public string? Suburb { get; set; }

		public string? Municipality { get; set; }

		public string? Neighbourhood { get; set; }

		public string? Quarter { get; set; }

		public string? Borough { get; set; }

		public string? Residential { get; set; }

		public string? Island { get; set; }

		public string? County { get; set; }

		public string? StateDistrict { get; set; }

		public string? State { get; set; }
	}

	private readonly HttpClient _httpClient;

	private readonly ILogger<PostcodeLookupService> _logger;

	private readonly string _openCageKey;

	private static readonly HashSet<string> ZippopotamCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"US", "DE", "CA", "FR", "ES", "IT", "NL", "AU", "AT", "BE",
		"BR", "CH", "DK", "FI", "JP", "MX", "NO", "NZ", "PL", "PT",
		"SE", "ZA"
	};

	private const int TimeoutSeconds = 5;

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	public PostcodeLookupService(HttpClient httpClient, ILogger<PostcodeLookupService> logger, IConfiguration config)
	{
		_httpClient = httpClient;
		_logger = logger;
		_openCageKey = config["OpenCage:ApiKey"];
	}

	public async Task<PostcodeLookupResponse> LookupAsync(string postcode, string countryCode = "GB", CancellationToken cancellationToken = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(postcode))
		{
			return Fail("Postcode is required");
		}
		if (string.IsNullOrWhiteSpace(countryCode))
		{
			return Fail("Unsupported country code");
		}
		string cleanPostcode = postcode.Trim().ToUpperInvariant();
		string normalizedCountry = countryCode.Trim().ToUpperInvariant();
		try
		{
			string text = normalizedCountry;
			if ((text == "GB" || text == "UK") ? true : false)
			{
				return await LookupUkAsync(cleanPostcode, cancellationToken);
			}
			if (normalizedCountry == "IN")
			{
				return await LookupIndiaAsync(cleanPostcode, cancellationToken);
			}
			if (ZippopotamCountries.Contains(normalizedCountry))
			{
				return await LookupZippopotamAsync(cleanPostcode, normalizedCountry, cancellationToken);
			}
			return await LookupNominatimAsync(cleanPostcode, normalizedCountry, cancellationToken);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			_logger.LogWarning("Postcode lookup timed out. Postcode: {Postcode}, Country: {Country}", cleanPostcode, normalizedCountry);
			return Fail("Network error validating postcode");
		}
		catch (HttpRequestException exception)
		{
			_logger.LogError(exception, "Postcode provider request failed. Postcode: {Postcode}, Country: {Country}", cleanPostcode, normalizedCountry);
			return Fail("Network error validating postcode");
		}
		catch (Exception exception2)
		{
			_logger.LogError(exception2, "Unexpected postcode lookup error. Postcode: {Postcode}, Country: {Country}", cleanPostcode, normalizedCountry);
			return Fail("Network error validating postcode");
		}
	}

	private async Task<PostcodeLookupResponse> LookupUkAsync(string postcode, CancellationToken cancellationToken)
	{
		string url = "https://api.postcodes.io/postcodes/" + Uri.EscapeDataString(postcode);
		using HttpResponseMessage response = await SendGetAsync(url, cancellationToken);
		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return Fail("Invalid postcode");
		}
		if (!response.IsSuccessStatusCode)
		{
			return Fail("Network error validating postcode");
		}
		PostcodesIoResult r = (await response.Content.ReadFromJsonAsync<PostcodesIoResponse>(JsonOptions, cancellationToken))?.Result;
		if (r == null)
		{
			return Fail("Invalid postcode");
		}
		PostcodeLookupData data = new PostcodeLookupData
		{
			postcode = r.Postcode,
			country = "United Kingdom",
			country_code = "GB",
			city = (r.AdminDistrict ?? r.Parish ?? r.AdminWard),
			district = r.AdminDistrict,
			region = r.Region,
			latitude = r.Latitude,
			longitude = r.Longitude,
			meta = new Dictionary<string, object>
			{
				["provider"] = "postcodes.io",
				["ward"] = r.AdminWard,
				["parish"] = r.Parish
			}
		};
		return Success(data);
	}

	private async Task<PostcodeLookupResponse> LookupIndiaAsync(string postcode, CancellationToken cancellationToken)
	{
		string pin = Regex.Replace(postcode, "\\s+", "");
		if (!Regex.IsMatch(pin, "^\\d{6}$"))
		{
			return Fail("Indian PIN codes must be 6 digits");
		}
		string url = "https://api.postalpincode.in/pincode/" + pin;
		using HttpResponseMessage response = await SendGetAsync(url, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			return Fail("Network error validating postcode");
		}
		IndiaApiResponse result = (await response.Content.ReadFromJsonAsync<List<IndiaApiResponse>>(JsonOptions, cancellationToken))?.FirstOrDefault();
		if (result?.Status != "Success" || result.PostOffice == null || result.PostOffice.Count == 0)
		{
			return Fail("Invalid postcode");
		}
		IndiaPostOffice primary = result.PostOffice[0];
		string city = ((!string.IsNullOrWhiteSpace(primary.Block) && !string.Equals(primary.Block, "NA", StringComparison.OrdinalIgnoreCase)) ? primary.Block : primary.District);
		if (primary.State == null)
		{
			_ = string.Empty;
		}
		List<string> officeNames = (from x in result.PostOffice
			select x.Name into x
			where !string.IsNullOrWhiteSpace(x)
			select x).Distinct().ToList();
		double? latitude = null;
		double? longitude = null;
		if (officeNames.Count == 1)
		{
			string officeName = officeNames[0];
			string query = $"{officeName}, {primary.District}, {primary.State}, India";
			(latitude, longitude) = await GeocodeWithOpenCageAsync(query, cancellationToken);
		}
		PostcodeLookupData data = new PostcodeLookupData
		{
			postcode = pin,
			country = "India",
			country_code = "IN",
			city = city,
			district = primary.District,
			region = primary.State,
			latitude = latitude,
			longitude = longitude,
			meta = new Dictionary<string, object>
			{
				["provider"] = "api.postalpincode.in",
				["offices"] = officeNames
			}
		};
		return Success(data);
	}

	public async Task<(double? Latitude, double? Longitude)> GeocodeWithOpenCageAsync(string query, CancellationToken cancellationToken)
	{
		string url = $"https://api.opencagedata.com/geocode/v1/json?q={Uri.EscapeDataString(query)}&key={_openCageKey}&limit=1";
		using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			return (Latitude: null, Longitude: null);
		}
		OpenCageResult result = (await response.Content.ReadFromJsonAsync<OpenCageResponse>(JsonOptions, cancellationToken))?.Results?.FirstOrDefault();
		if (result?.Geometry != null)
		{
			return (Latitude: result.Geometry.Lat, Longitude: result.Geometry.Lng);
		}
		return (Latitude: null, Longitude: null);
	}

	private async Task<(double? Latitude, double? Longitude)> ExecuteNominatimGeocodeAsync(string url, CancellationToken cancellationToken)
	{
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		request.Headers.UserAgent.ParseAdd("TradePlatform-PostcodeLookup/1.0");
		using HttpResponseMessage response = await SendAsync(request, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			return (Latitude: null, Longitude: null);
		}
		NominatimResult first = (await response.Content.ReadFromJsonAsync<List<NominatimResult>>(JsonOptions, cancellationToken))?.FirstOrDefault();
		if (first != null)
		{
			return (Latitude: ParseDouble(first.Lat), Longitude: ParseDouble(first.Lon));
		}
		return (Latitude: null, Longitude: null);
	}

	private async Task<PostcodeLookupResponse> LookupZippopotamAsync(string postcode, string countryCode, CancellationToken cancellationToken)
	{
		string clean = Regex.Replace(postcode, "[\\s-]+", "");
		if (countryCode == "US" && clean.Length >= 5)
		{
			clean = clean.Substring(0, 5);
		}
		string url = "https://api.zippopotam.us/" + countryCode.ToLowerInvariant() + "/" + Uri.EscapeDataString(clean);
		using HttpResponseMessage response = await SendGetAsync(url, cancellationToken);
		if (response.StatusCode == HttpStatusCode.NotFound)
		{
			return Fail("Invalid postcode");
		}
		if (!response.IsSuccessStatusCode)
		{
			return Fail("Network error validating postcode");
		}
		ZippopotamResponse json = await response.Content.ReadFromJsonAsync<ZippopotamResponse>(JsonOptions, cancellationToken);
		ZippopotamPlace place = json?.Places?.FirstOrDefault();
		if (place == null)
		{
			return Fail("No results found for this postcode");
		}
		PostcodeLookupData data = new PostcodeLookupData
		{
			postcode = (json?.PostCode ?? clean),
			country = json?.Country,
			country_code = countryCode,
			city = place.PlaceName,
			district = place.State,
			region = place.State,
			latitude = ParseDouble(place.Latitude),
			longitude = ParseDouble(place.Longitude),
			meta = new Dictionary<string, object> { ["provider"] = "zippopotam.us" }
		};
		return Success(data);
	}

	private async Task<PostcodeLookupResponse> LookupNominatimAsync(string postcode, string countryCode, CancellationToken cancellationToken)
	{
		string query = $"postalcode={Uri.EscapeDataString(postcode)}&countrycodes={Uri.EscapeDataString(countryCode.ToLowerInvariant())}&addressdetails=1&format=json";
		string url = "https://nominatim.openstreetmap.org/search?" + query;
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		request.Headers.UserAgent.ParseAdd("TradePlatform-PostcodeLookup/1.0");
		request.Headers.AcceptLanguage.ParseAdd("en");
		using HttpResponseMessage response = await SendAsync(request, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			return Fail("Network error validating postcode");
		}
		List<NominatimResult> json = await response.Content.ReadFromJsonAsync<List<NominatimResult>>(JsonOptions, cancellationToken);
		if (json == null || json.Count == 0)
		{
			return Fail("No results found for this postcode");
		}
		NominatimResult result = json[0];
		NominatimAddress address = result.Address;
		string city = address?.City ?? address?.Town ?? address?.Village ?? address?.Hamlet ?? address?.Locality ?? address?.Suburb ?? address?.Municipality ?? address?.Neighbourhood ?? address?.Quarter ?? address?.Borough ?? address?.Residential ?? address?.Island;
		PostcodeLookupData data = new PostcodeLookupData
		{
			postcode = postcode,
			country = (address?.Country ?? countryCode),
			country_code = countryCode,
			city = city,
			district = (address?.County ?? address?.StateDistrict),
			region = address?.State,
			latitude = ParseDouble(result.Lat),
			longitude = ParseDouble(result.Lon),
			meta = new Dictionary<string, object> { ["provider"] = "nominatim" }
		};
		return Success(data);
	}

	private async Task<HttpResponseMessage> SendGetAsync(string url, CancellationToken cancellationToken)
	{
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		return await SendAsync(request, cancellationToken);
	}

	private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		using CancellationTokenSource timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5.0));
		using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
		return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
	}

	private static PostcodeLookupResponse Success(PostcodeLookupData data)
	{
		return PostcodeLookupResponse.Ok(data);
	}

	private static PostcodeLookupResponse Fail(string message)
	{
		return PostcodeLookupResponse.Fail(message);
	}

	private static double? ParseDouble(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}
		if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return null;
		}
		return result;
	}
}
