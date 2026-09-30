using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TradePlatform.Api.Filters;

public class ValidationFilter : IActionFilter, IFilterMetadata
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (context.ModelState.IsValid)
		{
			return;
		}
		Dictionary<string, string[]> errors = context.ModelState.Where((KeyValuePair<string, ModelStateEntry> x) => x.Value.Errors.Count > 0).ToDictionary((KeyValuePair<string, ModelStateEntry> kvp) => kvp.Key, (KeyValuePair<string, ModelStateEntry> kvp) => kvp.Value.Errors.Select((ModelError e) => e.ErrorMessage).ToArray());
		var response = new
		{
			success = false,
			message = "Validation failed",
			type = "validation_error",
			errors = errors
		};
		context.Result = new BadRequestObjectResult(response);
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}
