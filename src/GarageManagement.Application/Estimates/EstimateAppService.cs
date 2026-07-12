using System;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Inventories;
using GarageManagement.Permissions;
using GarageManagement.ServiceOrders;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Estimates;

public class EstimateAppService : ReadOnlyAppService<Estimate, EstimateDto, Guid, EstimateGetListInputDto>,
    IEstimateAppService
{
    private readonly IEstimateRepository estimateRepository;
    private readonly IRepository<EstimateProductItem, Guid> estimateProductItemRepository;
    private readonly IServiceOrderRepository serviceOrderRepository;
    private readonly IInventoryAppService inventoryAppService;

    public EstimateAppService(
        IReadOnlyRepository<Estimate, Guid> repository,
        IEstimateRepository estimateRepository,
        IRepository<EstimateProductItem, Guid> estimateProductItemRepository,
        IServiceOrderRepository serviceOrderRepository,
        IInventoryAppService inventoryAppService) : base(repository)
    {
        this.estimateRepository = estimateRepository;
        this.estimateProductItemRepository = estimateProductItemRepository;
        this.serviceOrderRepository = serviceOrderRepository;
        this.inventoryAppService = inventoryAppService;

        GetPolicyName = GarageManagementPermissions.Estimates.Default;
        GetListPolicyName = GarageManagementPermissions.Estimates.Default;
    }

    [Authorize(GarageManagementPermissions.Estimates.Default)]
    public override async Task<EstimateDto> GetAsync(Guid id)
    {
        var estimate = await estimateRepository.GetWithDetailsAsync(id)
            ?? throw new UserFriendlyException("Orçamento não encontrado.");

        return ObjectMapper.Map<Estimate, EstimateDto>(estimate);
    }

    [Authorize(GarageManagementPermissions.Estimates.Approve)]
    public async Task<EstimateDto> ApproveAsync(Guid id)
    {
        var estimate = await estimateRepository.GetWithDetailsAsync(id)
            ?? throw new UserFriendlyException("Orçamento não encontrado.");

        var estimatePartItems = await estimateProductItemRepository.GetListAsync(item => item.EstimateId == estimate.Id);

        var quantitiesByProduct = estimatePartItems
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

        var serviceOrder = await serviceOrderRepository.FindByEstimateIdAsync(estimate.Id);
        if (serviceOrder != null && serviceOrder.Status is ServiceOrderStatus.WaitingApproval)
        {
            serviceOrder.WaitExecution();
            await serviceOrderRepository.UpdateAsync(serviceOrder, autoSave: true);
        }

        await estimateRepository.UpdateAsync(estimate, autoSave: true);

        return ObjectMapper.Map<Estimate, EstimateDto>(estimate);
    }

    [Authorize(GarageManagementPermissions.Estimates.Reject)]
    public async Task<EstimateDto> RejectAsync(Guid id, EstimateRejectDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            throw new UserFriendlyException("Informe o motivo da reprovação do orçamento.");
        }

        var estimate = await estimateRepository.GetWithDetailsAsync(id)
            ?? throw new UserFriendlyException("Orçamento não encontrado.");

        var estimatePartItems = await estimateProductItemRepository.GetListAsync(item => item.EstimateId == estimate.Id);

        var quantitiesByProduct = estimatePartItems
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

        var serviceOrder = await serviceOrderRepository.FindByEstimateIdAsync(estimate.Id);
        if (serviceOrder != null && serviceOrder.Status is not ServiceOrderStatus.Canceled and not ServiceOrderStatus.Closed)
        {
            serviceOrder.Cancel(input.Reason);
            await serviceOrderRepository.UpdateAsync(serviceOrder, autoSave: true);
        }

        await estimateRepository.UpdateAsync(estimate, autoSave: true);

        return ObjectMapper.Map<Estimate, EstimateDto>(estimate);
    }
}