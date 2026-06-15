using System;
using GarageManagement.Services;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Estimates;

public class EstimateServiceItemDto : EntityDto<Guid>
{
    public Guid ServiceId { get; set; }
    public virtual ServiceDto Service { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
