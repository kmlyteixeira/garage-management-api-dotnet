using System;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderGetListInputDto : PagedAndSortedResultRequestDto
{
    public Guid? EstimateId { get; set; }
    public ServiceOrderStatus? Status { get; set; }
}
