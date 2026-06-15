using System;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Permissions;
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
            GetPolicyName = GarageManagementPermissions.Inventories.Default;
            GetListPolicyName = GarageManagementPermissions.Inventories.Default;
            CreatePolicyName = GarageManagementPermissions.Inventories.Create;
            UpdatePolicyName = GarageManagementPermissions.Inventories.Edit;
            DeletePolicyName = GarageManagementPermissions.Inventories.Delete;
        }

        protected override async Task<IQueryable<Inventory>> CreateFilteredQueryAsync(InventoryGetListInputDto input)
        {
            var queryable = await base.CreateFilteredQueryAsync(input);

            if (input.ProductId.HasValue)
            {
                queryable = queryable.Where(inventory => inventory.ProductId == input.ProductId.Value);
            }

            return queryable;
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

            var inventory = await Repository.FirstOrDefaultAsync(
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
