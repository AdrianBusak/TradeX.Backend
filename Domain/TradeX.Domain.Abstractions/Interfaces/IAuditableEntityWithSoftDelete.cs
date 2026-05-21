namespace TradeX.Domain.Abstractions.Interfaces
{
    public interface IAuditableEntityWithSoftDelete
    {
        public bool IsActive { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? ModifiedByUserId { get; set; }
    }
}
