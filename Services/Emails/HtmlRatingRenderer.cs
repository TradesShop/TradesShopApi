using System;
using System.Text;

namespace TradePlatform.Api.Services.Emails;

public static class HtmlRatingRenderer
{
	private const string FullStarSvg = "<svg class=\"star star-full\" viewBox=\"0 0 24 24\"><path d=\"M12 .587l3.668 7.568L24 9.748l-6 5.848L19.335 24 12 19.897 4.665 24 6 15.596 0 9.748l8.332-1.593z\"/></svg>";

	private const string HalfStarSvg = "<svg class=\"star star-half\" viewBox=\"0 0 24 24\"><defs>  <linearGradient id=\"half\">    <stop offset=\"50%\" stop-color=\"#f5c518\"/>    <stop offset=\"50%\" stop-color=\"#ccc\"/>  </linearGradient></defs><path fill=\"url(#half)\" d=\"M12 .587l3.668 7.568L24 9.748l-6 5.848L19.335 24 12 19.897 4.665 24 6 15.596 0 9.748l8.332-1.593z\"/></svg>";

	private const string EmptyStarSvg = "<svg class=\"star star-empty\" viewBox=\"0 0 24 24\"><path fill=\"#ccc\" d=\"M12 .587l3.668 7.568L24 9.748l-6 5.848L19.335 24 12 19.897 4.665 24 6 15.596 0 9.748l8.332-1.593z\"/></svg>";

	public static string RenderRatingStarsHtml(double rating)
	{
		int fullStars = (int)Math.Floor(rating);
		bool hasHalfStar = rating % 1.0 >= 0.25 && rating % 1.0 < 0.75;
		int emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);
		StringBuilder html = new StringBuilder();
		for (int i = 0; i < fullStars; i++)
		{
			html.Append("<span style=\"color:#f5c518; font-size:22px;\">★</span>");
		}
		if (hasHalfStar)
		{
			html.Append("<span style=\"color:#f5c518; font-size:22px;\">⯨</span>");
		}
		for (int j = 0; j < emptyStars; j++)
		{
			html.Append("<span style=\"color:#ccc; font-size:22px;\">★</span>");
		}
		return html.ToString();
	}
}
