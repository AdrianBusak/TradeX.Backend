using API.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Net;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;

namespace API.Abstractions.Services;

public sealed class HttpRequestProcessingService : IHttpRequestProcessingService
{
    private static readonly JsonSerializerSettings JsonSettings = CreateJsonSettings();

    public async Task<ContentResult> ProcessHttpRequestAsync(
        Func<Task<IStandardResponse>> operation,
        ILogger log)
    {
        try
        {
            var response = await operation().ConfigureAwait(false);

            return IsErrorResult(response.Result)
                ? CreateErrorResult(response, log)
                : CreateJsonResult(response, ToHttpStatusCode(response.Result));
        }
        catch (JsonException ex)
        {
            return CreateErrorResult(
                new StandardResponse<string>(
                    OperationResult.BadRequest,
                    $"Json format error: {ex.Message}",
                    ex),
                log);
        }
        catch (Exception ex)
        {
            return CreateErrorResult(
                new StandardResponse<string>(
                    OperationResult.InternalError,
                    $"Unhandled error: {ex.Message}",
                    new ApplicationError(ex)),
                log);
        }
    }

    private static ContentResult CreateErrorResult(
        IStandardResponse response,
        ILogger log)
    {
        var wrappedResponse = WrapError(response, log);
        return CreateJsonResult(
            wrappedResponse,
            ToHttpStatusCode(response.Result));
    }

    private static StandardResponse<object> WrapError(
        IStandardResponse response,
        ILogger log)
    {
        return response.Error switch
        {
            ValidationErrorResponseModel validationError =>
                new StandardResponse<object>(
                    response.Result,
                    "ValidationError",
                    validationError.ValidationErrors),

            TokenExpiredResponseModel tokenExpiredError =>
                new StandardResponse<object>(
                    response.Result,
                    "TokenExpired",
                    tokenExpiredError),

            IError error =>
                LogAndWrap(
                    log,
                    response,
                    error.GetUserFriendlyMessage()),

            _ =>
                LogAndWrap(
                    log,
                    response,
                    response.Message ?? "An unexpected error occurred.")
        };
    }

    private static StandardResponse<object> LogAndWrap(
        ILogger log,
        IStandardResponse response,
        string message)
    {
        log.LogError(
            "Operation failed with result {Result}: {Message}",
            response.Result,
            message);

        return new StandardResponse<object>(
            response.Result,
            null!,
            message);
    }


    private static bool IsErrorResult(OperationResult result) =>
        result is OperationResult.InternalError
            or OperationResult.BadRequest
            or OperationResult.Unauthorized
            or OperationResult.Forbidden
            or OperationResult.NotFound
            or OperationResult.Conflict;

    private static ContentResult CreateJsonResult(
        object value,
        int statusCode)
        => new()
        {
            Content = JsonConvert.SerializeObject(value, JsonSettings),
            ContentType = "application/json",
            StatusCode = statusCode
        };

    private static int ToHttpStatusCode(OperationResult result) =>
        result switch
        {
            OperationResult.Ok
            or OperationResult.Updated
            or OperationResult.Deleted
                => (int)HttpStatusCode.OK,

            OperationResult.Created
                => (int)HttpStatusCode.Created,

            OperationResult.BadRequest
                => (int)HttpStatusCode.BadRequest,

            OperationResult.Unauthorized
                => (int)HttpStatusCode.Unauthorized,

            OperationResult.Forbidden
                => (int)HttpStatusCode.Forbidden,

            OperationResult.NotFound
                => (int)HttpStatusCode.NotFound,

            OperationResult.Conflict
                => (int)HttpStatusCode.Conflict,

            OperationResult.InternalError
                => (int)HttpStatusCode.InternalServerError,

            _ => (int)HttpStatusCode.OK
        };

    private static JsonSerializerSettings CreateJsonSettings() =>
        new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
}
