using System;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Inventories;
using GarageManagement.ServiceOrders;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Estimates;

public class EstimateAppService : ReadOnlyAppService<Estimate, EstimateDto, Guid, EstimateGetListInputDto>,
    IEstimateAppService
{
    private readonly IRepository<Estimate, Guid> estimateRepository;
    private readonly IRepository<ServiceOrder, Guid> serviceOrderRepository;
    private readonly IInventoryAppService inventoryAppService;

    public EstimateAppService(
        IReadOnlyRepository<Estimate, Guid> repository,
        IRepository<Estimate, Guid> estimateRepository,
        IRepository<ServiceOrder, Guid> serviceOrderRepository,
        IInventoryAppService inventoryAppService) : base(repository)
    {
        this.estimateRepository = estimateRepository;
        this.serviceOrderRepository = serviceOrderRepository;
        this.inventoryAppService = inventoryAppService;
    }

    public async Task<EstimateDto> ApproveAsync(Guid id)
    {
        var estimate = await estimateRepository.GetAsync(id, includeDetails: true)
            ?? throw new UserFriendlyException("Orçamento não encontrado.");

        var quantitiesByProduct = estimate.PartItems
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        foreach (var item in quantitiesByProduct)
        {
            await inventoryAppService.ConsumeReservedStockAsync(item.ProductId, item.Quantity);
        }

        estimate.Approve();

        var serviceOrder = await GetServiceOrderByEstimateIdAsync(estimate.Id);
        if (serviceOrder != null && serviceOrder.Status is ServiceOrderStatus.WaitingApproval)
        {
            serviceOrder.WaitExecution();
            await serviceOrderRepository.UpdateAsync(serviceOrder, autoSave: true);
        }

        await estimateRepository.UpdateAsync(estimate, autoSave: true);

        return ObjectMapper.Map<Estimate, EstimateDto>(estimate);
    }

    public async Task<EstimateDto> RejectAsync(Guid id, EstimateRejectDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            throw new UserFriendlyException("Informe o motivo da reprovação do orçamento.");
        }

        var estimate = await estimateRepository.GetAsync(id, includeDetails: true)
            ?? throw new UserFriendlyException("Orçamento não encontrado.");

        var quantitiesByProduct = estimate.PartItems
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        foreach (var item in quantitiesByProduct)
        {
            await inventoryAppService.ReleaseReservedStockAsync(item.ProductId, item.Quantity);
        }

        estimate.Reject(input.Reason);

        var serviceOrder = await GetServiceOrderByEstimateIdAsync(estimate.Id);
        if (serviceOrder != null && serviceOrder.Status is not ServiceOrderStatus.Canceled and not ServiceOrderStatus.Closed)
        {
            serviceOrder.Cancel(input.Reason);
            await serviceOrderRepository.UpdateAsync(serviceOrder, autoSave: true);
        }

        await estimateRepository.UpdateAsync(estimate, autoSave: true);

        return ObjectMapper.Map<Estimate, EstimateDto>(estimate);
    }

    private async Task<ServiceOrder?> GetServiceOrderByEstimateIdAsync(Guid estimateId)
    {
        var queryable = await serviceOrderRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(order => order.EstimateId == estimateId));
    }
}