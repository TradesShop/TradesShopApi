using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class SafeDateTimeBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext context)
	{
		string raw = context.ValueProvider.GetValue(context.ModelName).FirstValue;
		if (string.IsNullOrWhiteSpace(raw))
		{
			context.Result = ModelBindingResult.Success(null);
			return Task.CompletedTask;
		}
		if (DateTime.TryParse(raw, out var parsed))
		{
			context.Result = ModelBindingResult.Success(parsed);
			return Task.CompletedTask;
		}
		context.Result = ModelBindingResult.Success(null);
		return Task.CompletedTask;
	}
}
