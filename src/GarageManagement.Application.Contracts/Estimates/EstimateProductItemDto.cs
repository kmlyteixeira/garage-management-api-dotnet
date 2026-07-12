using System;
using GarageManagement.Products;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Estimates;

public class EstimateProductItemDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public virtual ProductDto Product { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
