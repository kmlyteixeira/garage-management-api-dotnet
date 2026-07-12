using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Estimates;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.ServiceOrders.Notifications;
using GarageManagement.Services;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderUpdateHandler : ApplicationService, IServiceOrderUpdateHandler
{
    private readonly IRepository<ServiceOrder, Guid> serviceOrderRepository;
    private readonly IRepository<Estimate, Guid> estimateRepository;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Service, Guid> serviceRepository;
    private readonly IRepository<Product, Guid> productRepository;

    private readonly IInventoryAppService inventoryAppService;
    private readonly IServiceOrderNotificationSender notificationSender;

    public ServiceOrderUpdateHandler(
        IRepository<ServiceOrder, Guid> serviceOrderRepository,
        IRepository<Estimate, Guid> estimateRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Service, Guid> serviceRepository,
        IRepository<Product, Guid> productRepository,
        IInventoryAppService inventoryAppService,
        IServiceOrderNotificationSender notificationSender)
    {
        this.serviceOrderRepository = serviceOrderRepository;
        this.estimateRepository = estimateRepository;
        this.customerRepository = customerRepository;
        this.serviceRepository = serviceRepository;
        this.productRepository = productRepository;
        this.inventoryAppService = inventoryAppService;
        this.notificationSender = notificationSender;
    }

    [RemoteService(false)]
    public async Task<ServiceOrderDto> HandleAsync(ServiceOrder serviceOrder, ServiceOrderUpdateDto input)
    {
        if (serviceOrder.Status is not ServiceOrderStatus.InDiagnosis)
        {
            throw new UserFriendlyException("Apenas ordens de serviço com status 'Em Diagnóstico' podem ser atualizadas.");
        }

        var estimate = await GetOrCreateEstimateAsync(serviceOrder);

        if (estimate.Status is not EstimateStatus.Approved && estimate.Status is not EstimateStatus.Rejected)
        {
            var estimateStatusBeforeUpdate = estimate.Status;

            await SyncServiceItems(estimate, input.ServiceItems);
            await SyncPartItems(estimate, input.PartItems);
            estimate.RecalculateTotals();

            var hasStockForAllPartItems = await HasStockForAllPartItemsAsync(input.PartItems);

            PromoteToPendingApprovalIfPossible(serviceOrder, estimate, hasStockForAllPartItems);

            if (estimateStatusBeforeUpdate == EstimateStatus.Draft && estimate.Status == EstimateStatus.PendingApproval)
            {
                await ReserveStockForEstimatePartItemsAsync(estimate);
            }

            await estimateRepository.UpdateAsync(estimate, autoSave: true);
            await serviceOrderRepository.UpdateAsync(serviceOrder, autoSave: true);

            if (estimateStatusBeforeUpdate == EstimateStatus.Draft && estimate.Status == EstimateStatus.PendingApproval)
            {
                await NotifyCustomerEstimatePendingApprovalAsync(estimate);
            }
        }

        return ObjectMapper.Map<ServiceOrder, ServiceOrderDto>(serviceOrder);
    }

    private async Task<Estimate> GetOrCreateEstimateAsync(ServiceOrder serviceOrder)
    {
        if (serviceOrder.EstimateId.HasValue)
        {
            return await estimateRepository.GetAsync(serviceOrder.EstimateId.Value, includeDetails: true);
        }

        var estimate = new Estimate(
            GuidGenerator.Create(),
            serviceOrder.ServiceOrderNumber,
            serviceOrder.CustomerId,
            serviceOrder.VehicleId
        );

        await estimateRepository.InsertAsync(estimate, autoSave: true);

        serviceOrder.AssociateEstimate(estimate.Id);

        await serviceOrderRepository.UpdateAsync(serviceOrder, autoSave: true);

        return estimate;
    }

    private async Task<bool> HasStockForAllPartItemsAsync(IReadOnlyCollection<ServiceOrderProductItemCreateDto> partItems)
    {
        if (partItems.Count == 0)
        {
            return false;
        }

        var quantitiesByProduct = partItems
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                RequiredQuantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        foreach (var product in quantitiesByProduct)
        {
            var isProductAvailable = await inventoryAppService.IsProductAvailableAsync(product.ProductId, product.RequiredQuantity);

            if (!isProductAvailable)
            {
                return false;
            }
        }

        return true;
    }

    private static void PromoteToPendingApprovalIfPossible(ServiceOrder serviceOrder, Estimate estimate, bool hasStockForAllPartItems)
    {
        if (!hasStockForAllPartItems)
        {
            return;
        }

        if (estimate.Status == EstimateStatus.Draft)
        {
            estimate.SendToCustomer();
        }

        if (serviceOrder.Status == ServiceOrderStatus.InDiagnosis)
        {
            serviceOrder.ChangeStatus(ServiceOrderStatus.WaitingApproval);
        }
    }

    private async Task SyncServiceItems(Estimate estimate, IReadOnlyCollection<ServiceOrderServiceItemCreateDto> items)
    {
        var incomingServiceIds = items.Select(i => i.ServiceId).Distinct().ToList();

        var servicesIncoming = await serviceRepository
            .GetListAsync(s => incomingServiceIds.Contains(s.Id));

        if (servicesIncoming.Count != incomingServiceIds.Count)
            throw new UserFriendlyException("Um ou mais serviços informados são inválidos.");
        
        var servicesToRemove = estimate.ServiceItems
            .Where(x => !incomingServiceIds.Contains(x.ServiceId)).ToList();

        foreach (var service in servicesToRemove)
        {
            estimate.RemoveServiceItem(service.Id);
        }

        foreach (var service in servicesIncoming)
        {
            var quantity = items.Where(i => i.ServiceId == service.Id).Sum(i => i.Quantity);

            if (estimate.ServiceItems.Any(x => x.ServiceId == service.Id))
            {
                estimate.UpdateServiceItem(service, quantity);
                continue;
            }

            estimate.AddServiceItem(service, quantity);
        }
    }

    private async Task SyncPartItems(Estimate estimate, IReadOnlyCollection<ServiceOrderProductItemCreateDto> items)
    {
        var incomingProductIds = items.Select(i => i.ProductId).Distinct().ToList();

        var productsIncoming = await productRepository
            .GetListAsync(p => incomingProductIds.Contains(p.Id));

        if (productsIncoming.Count != incomingProductIds.Count)
            throw new UserFriendlyException("Um ou mais produtos informados são inválidos.");

        var productsToRemove = estimate.PartItems
            .Where(x => !incomingProductIds.Contains(x.ProductId)).ToList();

        foreach (var product in productsToRemove)
        {
            estimate.RemovePartItem(product.Id);
        }

        foreach (var product in productsIncoming)
        {
            var quantity = items.Where(i => i.ProductId == product.Id).Sum(i => i.Quantity);

            if (estimate.PartItems.Any(x => x.ProductId == product.Id))
            {
                estimate.UpdatePartItem(product, quantity);
                continue;
            }

            estimate.AddPartItem(product, quantity);
        }
    }

    private async Task ReserveStockForEstimatePartItemsAsync(Estimate estimate)
    {
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
            await inventoryAppService.ReserveStockAsync(item.ProductId, item.Quantity);
        }
    }

    private async Task NotifyCustomerEstimatePendingApprovalAsync(Estimate estimate)
    {
        var customer = await customerRepository.GetAsync(estimate.CustomerId);
        await notificationSender.NotifyEstimatePendingApprovalAsync(estimate, customer);
    }
}