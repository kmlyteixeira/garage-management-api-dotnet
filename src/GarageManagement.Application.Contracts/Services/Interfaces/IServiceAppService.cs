using System;
using Volo.Abp.Application.Services;

namespace GarageManagement.Services;

public interface IServiceAppService : ICrudAppService<ServiceDto, Guid, ServiceGetListInputDto, ServiceCreateUpdateDto>
{
}
