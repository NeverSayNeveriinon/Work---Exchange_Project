using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace Core.Helpers;
//todo: make Constant "Type" 
public static class FluentResultsExtensions
{
    //
    public static bool IsStatusCode(this IError? error, string statusCode)
    {
        ArgumentNullException.ThrowIfNull(statusCode,$"The '{nameof(statusCode)}' parameter is Null");
        
        return error?.HasMetadata("Type", val => val.Equals(statusCode)) ?? false;
    }
    
    public static IError CreateError(string statusCode, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(statusCode,$"The '{nameof(statusCode)}' parameter is Null");

        return new Error(message).WithMetadata("Type", statusCode);
    }
    public static IError CreateNotFoundError(string? message = null)
    {
        return new Error(message).WithMetadata("Type", nameof(StatusCodes.Status404NotFound));
    }
    
    public static (Result<T>, T) DeconstructObject<T>(this Result<T>? res)
    {
        return (res, res.ValueOrDefault);
    }
    
    
    // First Messages //
    public static string? FirstErrorMessage(this Result? res)
    {
        return res?.Errors.FirstOrDefault()?.Message;
    }
    public static string? FirstErrorMessage<T>(this Result<T>? res)
    {
        return res?.Errors.FirstOrDefault()?.Message;
    }
    //
    public static string? FirstSuccessMessage(this Result? res)
    {
        return res?.Successes.FirstOrDefault()?.Message;
    }
    public static string? FirstSuccessMessage<T>(this Result<T>? res)
    {
        return res?.Successes.FirstOrDefault()?.Message;
    }
}