using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Lootopia.Api.SharedKernel.Results;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return HttpResults.NoContent();

        return MapErrorToHttpResult(result.Error);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return HttpResults.Ok(result.Value);

        return MapErrorToHttpResult(result.Error);
    }

    public static IResult ToCreatedHttpResult<T>(this Result<T> result, string uri)
    {
        if (result.IsSuccess)
            return HttpResults.Created(uri, result.Value);

        return MapErrorToHttpResult(result.Error);
    }

    private static IResult MapErrorToHttpResult(Error error)
    {
        return error.Code switch
        {
            "General.NotFound" => HttpResults.NotFound(error),
            "General.Validation" => HttpResults.BadRequest(error),
            "General.Conflict" => HttpResults.Conflict(error),
            "General.Forbidden" => HttpResults.Json(error, statusCode: 403),
            "General.Unauthorized" => HttpResults.Json(error, statusCode: 401),
            _ when error.Code.Contains("Validation") => HttpResults.BadRequest(error),
            _ when error.Code.Contains("NotFound") => HttpResults.NotFound(error),
            _ when error.Code.Contains("Forbidden") => HttpResults.Json(error, statusCode: 403),
            _ when error.Code.Contains("Unauthorized") => HttpResults.Json(error, statusCode: 401),
            _ when error.Code.Contains("InsufficientBalance") => HttpResults.Json(error, statusCode: 402),
            _ => HttpResults.Json(error, statusCode: 500)
        };
    }
}
