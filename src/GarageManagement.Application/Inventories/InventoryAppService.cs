using System;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Inventories
{
    public class InventoryAppService :
        CrudAppService<Inventory, InventoryDto, Guid, InventoryGetListInputDto, InventoryCreateUpdateDto>,
        IInventoryAppService
    {
        public InventoryAppService(IRepository<Inventory, Guid> repository) : base(repository)
        {
        }
    }
}