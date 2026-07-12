using System;
using Volo.Abp.Application.Dtos;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderGetListInputDto : PagedAndSortedResultRequestDto
{
    public Guid? EstimateId { get; set; }
    public ServiceOrderStatus? Status { get; set; }

    /// <summary>
    /// When false (default), service orders with status Finished or Delivered are excluded
    /// from the listing (logical exclusion only, entities remain fully queryable via GetAsync).
    /// </summary>
    public bool IncludeFinalized { get; set; }
}
