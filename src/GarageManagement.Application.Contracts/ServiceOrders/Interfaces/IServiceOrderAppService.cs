using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GarageManagement.ServiceOrders;

public interface IServiceOrderAppService : ICrudAppService<ServiceOrderDto, Guid, ServiceOrderGetListInputDto, ServiceOrderCreateDto, ServiceOrderUpdateDto>
{
	Task<ServiceOrderPublicStatusDto> GetPublicStatusAsync(ServiceOrderPublicStatusRequestDto input);
}
