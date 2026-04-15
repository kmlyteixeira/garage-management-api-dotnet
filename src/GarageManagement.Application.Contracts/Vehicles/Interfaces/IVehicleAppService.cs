using System;
using Volo.Abp.Application.Services;

namespace GarageManagement.Vehicles;

public interface IVehicleAppService : ICrudAppService<VehicleDto, Guid, VehicleGetListInputDto, VehicleCreateUpdateDto>
{
}
