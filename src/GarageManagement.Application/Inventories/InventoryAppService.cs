using System;
using System.Threading.Tasks;
using Volo.Abp;
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

        [RemoteService(false)]
        public async Task<bool> IsProductAvailableAsync(Guid productId, int requiredQuantity)
        {
            if (requiredQuantity <= 0)
            {
                return true;
            }

            var queryable = await Repository.GetQueryableAsync();
            var hasStock = await AsyncExecuter.AnyAsync(
                queryable,
                inventory => inventory.ProductId == productId && (inventory.Quantity - inventory.ReservedQuantity) >= requiredQuantity
            );

            return hasStock;
        }

        [RemoteService(false)]
        public async Task DecreaseStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            var queryable = await Repository.GetQueryableAsync();
            var inventory = await AsyncExecuter.FirstOrDefaultAsync(
                queryable,
                inventory => inventory.ProductId == productId
            );

            if (inventory == null)
            {
                throw new UserFriendlyException("Produto não encontrado no estoque.");
            }

            if (inventory.Quantity < quantity)
            {
                throw new UserFriendlyException("Quantidade insuficiente em estoque.");
            }

            inventory.DecreaseStock(quantity);

            await Repository.UpdateAsync(inventory, autoSave: true);
        }

        [RemoteService(false)]
        public async Task ReserveStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            var queryable = await Repository.GetQueryableAsync();
            var inventory = await AsyncExecuter.FirstOrDefaultAsync(
                queryable,
                item => item.ProductId == productId
            );

            if (inventory == null)
            {
                throw new UserFriendlyException("Produto não encontrado no estoque.");
            }

            inventory.ReserveStock(quantity);

            await Repository.UpdateAsync(inventory, autoSave: true);
        }

        [RemoteService(false)]
        public async Task ReleaseReservedStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            var queryable = await Repository.GetQueryableAsync();
            var inventory = await AsyncExecuter.FirstOrDefaultAsync(
                queryable,
                item => item.ProductId == productId
            );

            if (inventory == null)
            {
                throw new UserFriendlyException("Produto não encontrado no estoque.");
            }

            inventory.ReleaseReservedStock(quantity);

            await Repository.UpdateAsync(inventory, autoSave: true);
        }

        [RemoteService(false)]
        public async Task ConsumeReservedStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                return;
            }

            var queryable = await Repository.GetQueryableAsync();
            var inventory = await AsyncExecuter.FirstOrDefaultAsync(
                queryable,
                item => item.ProductId == productId
            );

            if (inventory == null)
            {
                throw new UserFriendlyException("Produto não encontrado no estoque.");
            }

            inventory.ConsumeReservedStock(quantity);

            await Repository.UpdateAsync(inventory, autoSave: true);
        }
    }
}
