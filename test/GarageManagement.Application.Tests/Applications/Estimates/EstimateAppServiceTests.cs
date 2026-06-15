using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using GarageManagement.Estimates;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.ServiceOrders;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace GarageManagement.Application.Tests.Applications.Estimates;

public class EstimateAppServiceTests
{
    private static void SetObjectMapper(EstimateAppService sut, IObjectMapper objectMapper)
    {
        typeof(Volo.Abp.Application.Services.ApplicationService)
            .GetProperty("ObjectMapper", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?.SetValue(sut, objectMapper);
    }

    private static IAsyncQueryableExecuter CreateInMemoryExecuter()
    {
        var executer = Substitute.For<IAsyncQueryableExecuter>();

        executer
            .FirstOrDefaultAsync(Arg.Any<IQueryable<ServiceOrder>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(((IQueryable<ServiceOrder>)ci[0]).FirstOrDefault()));

        return executer;
    }

    private static EstimateAppService CreateSut(
        IEstimateRepository? estimateRepository = null,
        IRepository<EstimateProductItem, Guid>? estimateProductItemRepository = null,
        IRepository<ServiceOrder, Guid>? serviceOrderRepository = null,
        IInventoryAppService? inventoryAppService = null)
    {
        var sut = new EstimateAppService(
            Substitute.For<IReadOnlyRepository<Estimate, Guid>>(),
            estimateRepository ?? Substitute.For<IEstimateRepository>(),
            estimateProductItemRepository ?? Substitute.For<IRepository<EstimateProductItem, Guid>>(),
            serviceOrderRepository ?? Substitute.For<IRepository<ServiceOrder, Guid>>(),
            inventoryAppService ?? Substitute.For<IInventoryAppService>());

        var executer = CreateInMemoryExecuter();
        var lazyProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyProvider.LazyGetRequiredService<IAsyncQueryableExecuter>().Returns(executer);
        sut.LazyServiceProvider = lazyProvider;

        return sut;
    }

    [Fact]
    public async Task ApproveAsync_Should_Throw_When_Estimate_Not_Found()
    {
        var repo = Substitute.For<IEstimateRepository>();
        var sut = CreateSut(estimateRepository: repo);

        repo.GetWithDetailsAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Estimate>(null!));

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.ApproveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ApproveAsync_Should_Consume_Grouped_Reserved_Stock_And_Move_ServiceOrder_To_WaitingExecution()
    {
        var estimateRepository = Substitute.For<IEstimateRepository>();
        var estimateProductItemRepository = Substitute.For<IRepository<EstimateProductItem, Guid>>();
        var serviceOrderRepository = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var inventoryAppService = Substitute.For<IInventoryAppService>();

        var estimate = new Estimate(Guid.NewGuid(), "EST-1", Guid.NewGuid(), Guid.NewGuid());
        var product = new Product("Filtro", 35m);
        estimate.AddPartItem(product, 2);
        estimate.AddPartItem(product, 3);
        estimate.SendToCustomer();
        var partItems = estimate.PartItems.ToList();

        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-1", estimate.CustomerId, estimate.VehicleId);
        serviceOrder.AssociateEstimate(estimate.Id);
        serviceOrder.WaitApproval();

        estimateRepository.GetWithDetailsAsync(estimate.Id).Returns(Task.FromResult(estimate));
        estimateProductItemRepository.GetListAsync(default!, default, default).ReturnsForAnyArgs(Task.FromResult(partItems));
        serviceOrderRepository.GetQueryableAsync().Returns(Task.FromResult(new List<ServiceOrder> { serviceOrder }.AsQueryable()));

        var sut = CreateSut(estimateRepository, estimateProductItemRepository, serviceOrderRepository, inventoryAppService);
        var objectMapper = Substitute.For<IObjectMapper>();
        objectMapper.Map<Estimate, EstimateDto>(Arg.Any<Estimate>()).Returns(new EstimateDto());
        SetObjectMapper(sut, objectMapper);

        var result = await sut.ApproveAsync(estimate.Id);

        result.Status.ShouldBe(EstimateStatus.Approved);
        estimate.Status.ShouldBe(EstimateStatus.Approved);
        serviceOrder.Status.ShouldBe(ServiceOrderStatus.WaitingExecution);
        await inventoryAppService.Received(1).ConsumeReservedStockAsync(product.Id, 5);
        await serviceOrderRepository.Received(1).UpdateAsync(serviceOrder, autoSave: true);
        await estimateRepository.Received(1).UpdateAsync(estimate, autoSave: true);
    }

    [Fact]
    public async Task RejectAsync_Should_Throw_When_Reason_Empty()
    {
        var sut = CreateSut();

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.RejectAsync(Guid.NewGuid(), new EstimateRejectDto { Reason = " " }));
    }

    [Fact]
    public async Task RejectAsync_Should_Release_Grouped_Reserved_Stock_And_Cancel_ServiceOrder()
    {
        var estimateRepository = Substitute.For<IEstimateRepository>();
        var estimateProductItemRepository = Substitute.For<IRepository<EstimateProductItem, Guid>>();
        var serviceOrderRepository = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var inventoryAppService = Substitute.For<IInventoryAppService>();

        var estimate = new Estimate(Guid.NewGuid(), "EST-2", Guid.NewGuid(), Guid.NewGuid());
        var product = new Product("Pastilha", 90m);
        estimate.AddPartItem(product, 1);
        estimate.AddPartItem(product, 2);
        estimate.SendToCustomer();
        var partItems = estimate.PartItems.ToList();

        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "OS-2", estimate.CustomerId, estimate.VehicleId);
        serviceOrder.AssociateEstimate(estimate.Id);
        serviceOrder.WaitApproval();

        estimateRepository.GetWithDetailsAsync(estimate.Id).Returns(Task.FromResult(estimate));
        estimateProductItemRepository.GetListAsync(default!, default, default).ReturnsForAnyArgs(Task.FromResult(partItems));
        serviceOrderRepository.GetQueryableAsync().Returns(Task.FromResult(new List<ServiceOrder> { serviceOrder }.AsQueryable()));

        var sut = CreateSut(estimateRepository, estimateProductItemRepository, serviceOrderRepository, inventoryAppService);
        var objectMapper = Substitute.For<IObjectMapper>();
        objectMapper.Map<Estimate, EstimateDto>(Arg.Any<Estimate>()).Returns(new EstimateDto());
        SetObjectMapper(sut, objectMapper);

        var result = await sut.RejectAsync(estimate.Id, new EstimateRejectDto { Reason = "Cliente recusou" });

        result.Status.ShouldBe(EstimateStatus.Rejected);
        estimate.Status.ShouldBe(EstimateStatus.Rejected);
        estimate.RejectionReason.ShouldBe("Cliente recusou");
        serviceOrder.Status.ShouldBe(ServiceOrderStatus.Canceled);
        await inventoryAppService.Received(1).ReleaseReservedStockAsync(product.Id, 3);
        await serviceOrderRepository.Received(1).UpdateAsync(serviceOrder, autoSave: true);
        await estimateRepository.Received(1).UpdateAsync(estimate, autoSave: true);
    }
}
