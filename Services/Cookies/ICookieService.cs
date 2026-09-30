using Microsoft.AspNetCore.Http;

namespace TradePlatform.Api.Services.Cookies;

public interface ICookieService
{
	void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken);

	void ClearAuthCookies(HttpResponse response);
}
