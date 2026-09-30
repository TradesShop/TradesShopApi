using System;

namespace TradePlatform.Api.Infrastructure.Serialization;

[AttributeUsage(AttributeTargets.Property)]
public class NoDateTimeConversionAttribute : Attribute
{
}
