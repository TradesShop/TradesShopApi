using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace TradePlatform.Api.Infrastructure.Serialization;

public class SafeDateTimeBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
		{
			return new BinderTypeModelBinder(typeof(SafeDateTimeBinder));
		}
		return null;
	}
}
