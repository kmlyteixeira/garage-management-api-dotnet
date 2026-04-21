using System;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.Estimates;

public class EstimateGetListInputDto : PagedAndSortedResultRequestDto
{
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public EstimateStatus? Status { get; set; }
}
