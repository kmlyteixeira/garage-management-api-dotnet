using System;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderDto : EntityDto<Guid>
{
    public string ServiceOrderNumber { get; set; } = string.Empty;
    public Guid? EstimateId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public ServiceOrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DiagnosisStartedAt { get; set; }
    public DateTime? ExecutionStartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? CancellationReason { get; set; }
}
