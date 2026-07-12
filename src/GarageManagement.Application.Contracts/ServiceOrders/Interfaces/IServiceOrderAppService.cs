using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GarageManagement.ServiceOrders;

public interface IServiceOrderAppService : ICrudAppService<ServiceOrderDto, Guid, ServiceOrderGetListInputDto, ServiceOrderCreateDto, ServiceOrderUpdateDto>
{
	Task<ServiceOrderPublicStatusDto> GetPublicStatusAsync(ServiceOrderPublicStatusRequestDto input);

	/// <summary>
	/// Opens a new service order in a single call: creates it, starts diagnosis and,
	/// when service/part items are informed, builds the linked estimate right away.
	/// </summary>
	Task<ServiceOrderDto> OpenAsync(ServiceOrderOpenDto input);
}
