using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GarageManagement.Inventories;

public interface IInventoryAppService : ICrudAppService<InventoryDto, Guid, InventoryGetListInputDto, InventoryCreateUpdateDto>
{
    Task<bool> IsProductAvailableAsync(Guid productId, int requiredQuantity);
    Task DecreaseStockAsync(Guid productId, int quantity);
    Task ReserveStockAsync(Guid productId, int quantity);
    Task ReleaseReservedStockAsync(Guid productId, int quantity);
    Task ConsumeReservedStockAsync(Guid productId, int quantity);
}

