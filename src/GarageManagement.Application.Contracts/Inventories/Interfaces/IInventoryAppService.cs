using System;
using Volo.Abp.Application.Services;

namespace GarageManagement.Inventories;

public interface IInventoryAppService : ICrudAppService<InventoryDto, Guid, InventoryGetListInputDto, InventoryCreateUpdateDto>
{
}
