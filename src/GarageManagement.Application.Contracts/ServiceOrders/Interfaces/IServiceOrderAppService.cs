using System;
using Volo.Abp.Application.Services;

namespace GarageManagement.ServiceOrders;

public interface IServiceOrderAppService : ICrudAppService<ServiceOrderDto, Guid, ServiceOrderGetListInputDto, ServiceOrderCreateDto, ServiceOrderUpdateDto>
{
}
