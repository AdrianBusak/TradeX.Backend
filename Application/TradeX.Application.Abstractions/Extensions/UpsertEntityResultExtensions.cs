using TradeX.Repository.Abstractions.Enums;
using TradeX.Application.Abstractions.Enums;

namespace TradeX.Application.Abstractions.Extensions;

public static class UpsertEntityResultExtensions
{
    public static OperationResult ToOperationResult(this UpsertEntityResult upsertEntityResult)
    {
        return upsertEntityResult switch
        {
            UpsertEntityResult.Updated => OperationResult.Updated,
            UpsertEntityResult.Inserted => OperationResult.Created,
            UpsertEntityResult.Unchanged => OperationResult.Ok,
            _ => throw new NotImplementedException(),
        };
    }
}
