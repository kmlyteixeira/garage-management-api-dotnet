using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Estimates;

public class EstimateDto : EntityDto<Guid>
{
    public string EstimateNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public EstimateStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? ConvertedToServiceOrderId { get; set; }
    public List<EstimateServiceItemDto> ServiceItems { get; set; } = new();
    public List<EstimateProductItemDto> PartItems { get; set; } = new();
}
