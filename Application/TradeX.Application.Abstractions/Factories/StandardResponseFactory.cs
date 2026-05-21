using Azure.Core;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;

namespace TradeX.Application.Abstractions.Factories;
public static class StandardResponseFactory 
{
    public static TResponse CreateError<TResponse>(
        OperationResult result,
        string message,
        object error)
        where TResponse : IStandardResponse
    {
        return (TResponse)Activator.CreateInstance(
            typeof(TResponse)!,
            result,
            message,
            error
        )!;
    }
    
    public static StandardResponse<TResponseModel> CreateCustomStandardResponse<TResponseModel>(OperationResult result, string message)
    {
        return new StandardResponse<TResponseModel>(
            result,
            message,
            null!);
    }

    public static StandardResponse<TResponseModel> CreateEntityNotFoundStandardResponse<TResponseModel>(Guid entityId, string entityTypeName)
    {
        return new StandardResponse<TResponseModel>(
            OperationResult.NotFound,
            $"Entity with the given key not found. [Id: {entityId}]] [EntityType: {entityTypeName}]",
            null!);
    }

    public static StandardResponse<TResponseModel> CreateEntityNotInTenantStandardResponse<TResponseModel>(Guid entityId, Guid entityTenantId, Guid TenantId, string entityTypeName)
    {
        return new StandardResponse<TResponseModel>(
            OperationResult.BadRequest,
            $"Entity belongs to another Tenant. [Id: {entityId}]] [TenantId: {entityTenantId}] [ActiveTenantId: {TenantId}] [EntityType: {entityTypeName}]",
            null!);
    }

    public static StandardResponse<TResponseModel> CreateEntityAlreadyDeletedStandardResponse<TResponseModel>(Guid entityId, string entityTypeName)
    {
        return new StandardResponse<TResponseModel>(
            OperationResult.NotFound,
            $"Entity with the given key is already deleted. [Id: {entityId}]] [EntityType: {entityTypeName}]",
            null!);
    }

    public static StandardResponse<TResponseModel> CreateEntityAlreadyRestoredStandardResponse<TResponseModel>(Guid entityId, string entityTypeName)
    {
        return new StandardResponse<TResponseModel>(
            OperationResult.NotFound,
            $"Entity with the given key is already restored. [Id: {entityId}]] [EntityType: {entityTypeName}]",
            null!);
    }

    public static StandardResponse<TResponseModel> CreateEntityAlreadyExistsStandardResponse<TResponseModel>(Guid entityId, string entityTypeName, string fieldName, string fieldValue)
    {
        return new StandardResponse<TResponseModel>(
            OperationResult.Conflict,
            $"Entity with the given key already exists. [Id: {entityId}]] [EntityType: {entityTypeName}] [FieldName: {fieldName}] [FieldValue: {fieldValue}]",
            null!);
    }

    public static StandardResponse<CreateEntityResponseModel> CreateEntityStandardResponse(Guid entityId)
    {
        return new StandardResponse<CreateEntityResponseModel>(
                OperationResult.Created,
                new CreateEntityResponseModel() { Id = entityId });
    }
}
