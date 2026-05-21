namespace TradeX.Application.Abstractions.Enums;

public enum OperationResult
{
    Ok,

    Created,
    Updated,
    Deleted,
    NotFound,
    Conflict, 

    BadRequest,
    Unauthorized,
    Forbidden,
    
    InternalError
}