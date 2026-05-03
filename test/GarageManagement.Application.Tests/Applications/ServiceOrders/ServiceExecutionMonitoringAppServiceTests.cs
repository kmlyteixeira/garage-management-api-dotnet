using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GarageManagement.Estimates;
using GarageManagement.ServiceOrders;
using GarageManagement.Services;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.Application.Tests.Applications.ServiceOrders;

public class ServiceExecutionMonitoringAppServiceTests
{
    private static void SetProperty(object obj, string name, object? value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }
            type = type.BaseType;
        }
    }

    private static Service CreateService(Guid id, string description, double estimatedTime)
    {
        var service = new Service(description, 100m, estimatedTime);
        SetProperty(service, "Id", id);
        return service;
    }

    private static ServiceOrder CreateServiceOrder(
        Guid id,
        Guid customerId,
        Guid vehicleId,
        ServiceOrderStatus status,
        Estimate? estimate = null,
        DateTime? executionStartedAt = null,
        DateTime? finishedAt = null)
    {
        var so = new ServiceOrder(id, $"SO-{id:N}", customerId, vehicleId);
        SetProperty(so, "Status", status);
        if (estimate != null)
        {
            SetProperty(so, "EstimateId", (Guid?)estimate.Id);
            SetProperty(so, "Estimate", estimate);
        }
        if (executionStartedAt.HasValue)
            SetProperty(so, "ExecutionStartedAt", executionStartedAt);
        if (finishedAt.HasValue)
            SetProperty(so, "FinishedAt", finishedAt);
        return so;
    }

    private static ServiceExecutionMonitoringAppService CreateSut(
        IServiceOrderRepository? serviceOrderRepo = null,
        IRepository<Service, Guid>? serviceRepo = null)
    {
        return new ServiceExecutionMonitoringAppService(
            serviceOrderRepo ?? Substitute.For<IServiceOrderRepository>(),
            serviceRepo ?? Substitute.For<IRepository<Service, Guid>>());
    }

    // ── GetAllServicesMonitoringAsync ──────────────────────────────────────

    [Fact]
    public async Task GetAllServicesMonitoringAsync_ReturnsEmptyOverview_WhenNoCompletedOrders()
    {
        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder>().AsQueryable()));

        var sut = CreateSut(serviceOrderRepo: serviceOrderRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.ShouldNotBeNull();
        result.Services.ShouldBeEmpty();
        result.TotalServiceOrders.ShouldBe(0);
        result.OverallAverageExecutionTimeInHours.ShouldBe(0);
        result.OverallAverageEstimatedTimeInHours.ShouldBe(0);
        result.AverageAccuracyPercentage.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllServicesMonitoringAsync_IgnoresNonCompletedStatusOrders()
    {
        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        var openOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceOrderStatus.InExecution);
        var receivedOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceOrderStatus.Received);

        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { openOrder, receivedOrder }.AsQueryable()));

        var sut = CreateSut(serviceOrderRepo: serviceOrderRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.ShouldBeEmpty();
        result.TotalServiceOrders.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllServicesMonitoringAsync_ReturnsOverview_WithCorrectMetrics()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Oil Change", estimatedTime: 1.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-001", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var start = DateTime.UtcNow.AddHours(-3);
        var end = start.AddHours(2);
        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estimate, start, end);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.Count.ShouldBe(1);
        result.TotalServiceOrders.ShouldBe(1);

        var monitoring = result.Services[0];
        monitoring.ServiceId.ShouldBe(serviceId);
        monitoring.ServiceName.ShouldBe("Oil Change");
        monitoring.EstimatedTimeInHours.ShouldBe(1.0);
        monitoring.AverageRealExecutionTimeInHours.ShouldBe(2.0, tolerance: 0.01);
        monitoring.VarianceInHours.ShouldBe(1.0, tolerance: 0.01);
        monitoring.VariancePercentage.ShouldBe(100.0, tolerance: 0.01);
        monitoring.ExecutionCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetAllServicesMonitoringAsync_IncludesAllCompletedStatuses()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Full Service", estimatedTime: 2.0);

        var est1 = new Estimate(Guid.NewGuid(), "EST-S1", Guid.NewGuid(), Guid.NewGuid());
        est1.AddServiceItem(service, 1);
        var est2 = new Estimate(Guid.NewGuid(), "EST-S2", Guid.NewGuid(), Guid.NewGuid());
        est2.AddServiceItem(service, 1);
        var est3 = new Estimate(Guid.NewGuid(), "EST-S3", Guid.NewGuid(), Guid.NewGuid());
        est3.AddServiceItem(service, 1);

        var now = DateTime.UtcNow;
        var finishedOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, est1, now.AddHours(-3), now.AddHours(-1));
        var deliveredOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Delivered, est2, now.AddHours(-5), now.AddHours(-3));
        var closedOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Closed, est3, now.AddHours(-7), now.AddHours(-5));

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { finishedOrder, deliveredOrder, closedOrder }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(finishedOrder.Id).Returns(Task.FromResult(finishedOrder));
        serviceOrderRepo.GetWithDetailsAsync(deliveredOrder.Id).Returns(Task.FromResult(deliveredOrder));
        serviceOrderRepo.GetWithDetailsAsync(closedOrder.Id).Returns(Task.FromResult(closedOrder));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.Count.ShouldBe(1);
        result.Services[0].ExecutionCount.ShouldBe(3);
    }

    // ── GetServiceMonitoringAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetServiceMonitoringAsync_ReturnsZeroData_WhenNoRecordsExist()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Tire Rotation", 0.5);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder>().AsQueryable()));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetServiceMonitoringAsync(serviceId);

        result.ShouldNotBeNull();
        result.ServiceId.ShouldBe(serviceId);
        result.ServiceName.ShouldBe("Tire Rotation");
        result.EstimatedTimeInHours.ShouldBe(0.5);
        result.AverageRealExecutionTimeInHours.ShouldBe(0);
        result.ExecutionCount.ShouldBe(0);
        result.VarianceInHours.ShouldBe(0);
        result.VariancePercentage.ShouldBe(0);
    }

    [Fact]
    public async Task GetServiceMonitoringAsync_ReturnsMonitoringData_WhenRecordsExist()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Brake Inspection", estimatedTime: 2.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-002", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var start = DateTime.UtcNow.AddHours(-4);
        var end = start.AddHours(3);
        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Delivered, estimate, start, end);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetServiceMonitoringAsync(serviceId);

        result.ShouldNotBeNull();
        result.ServiceId.ShouldBe(serviceId);
        result.AverageRealExecutionTimeInHours.ShouldBe(3.0, tolerance: 0.01);
        result.ExecutionCount.ShouldBe(1);
    }

    // ── GetCustomerServiceMonitoringAsync ──────────────────────────────────

    [Fact]
    public async Task GetCustomerServiceMonitoringAsync_FiltersOrders_ByCustomerId()
    {
        var customerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Engine Check", estimatedTime: 4.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-003", customerId, Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var start = DateTime.UtcNow.AddHours(-5);
        var end = start.AddHours(4);

        var matchingOrder = CreateServiceOrder(Guid.NewGuid(), customerId, Guid.NewGuid(),
            ServiceOrderStatus.Closed, estimate, start, end);

        var otherOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { matchingOrder, otherOrder }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(matchingOrder.Id).Returns(Task.FromResult(matchingOrder));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetCustomerServiceMonitoringAsync(customerId);

        result.TotalServiceOrders.ShouldBe(1);
        result.Services.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetCustomerServiceMonitoringAsync_ReturnsEmpty_WhenNoMatchingOrders()
    {
        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder>().AsQueryable()));

        var sut = CreateSut(serviceOrderRepo: serviceOrderRepo);

        var result = await sut.GetCustomerServiceMonitoringAsync(Guid.NewGuid());

        result.Services.ShouldBeEmpty();
        result.TotalServiceOrders.ShouldBe(0);
    }

    // ── GetVehicleServiceMonitoringAsync ───────────────────────────────────

    [Fact]
    public async Task GetVehicleServiceMonitoringAsync_FiltersOrders_ByVehicleId()
    {
        var vehicleId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "AC Service", estimatedTime: 3.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-004", Guid.NewGuid(), vehicleId);
        estimate.AddServiceItem(service, 1);

        var start = DateTime.UtcNow.AddHours(-4);
        var end = start.AddHours(3);

        var matchingOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), vehicleId,
            ServiceOrderStatus.Finished, estimate, start, end);

        var otherOrder = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { matchingOrder, otherOrder }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(matchingOrder.Id).Returns(Task.FromResult(matchingOrder));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetVehicleServiceMonitoringAsync(vehicleId);

        result.TotalServiceOrders.ShouldBe(1);
        result.Services.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetVehicleServiceMonitoringAsync_ReturnsEmpty_WhenNoMatchingOrders()
    {
        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder>().AsQueryable()));

        var sut = CreateSut(serviceOrderRepo: serviceOrderRepo);

        var result = await sut.GetVehicleServiceMonitoringAsync(Guid.NewGuid());

        result.Services.ShouldBeEmpty();
    }

    // ── Edge cases in GetServiceMonitoringDataAsync ────────────────────────

    [Fact]
    public async Task GetServiceMonitoringDataAsync_SkipsOrder_WhenMissingExecutionStartedAt()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Alignment", 1.0);
        var estimate = new Estimate(Guid.NewGuid(), "EST-005", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estimate,
            executionStartedAt: null, finishedAt: DateTime.UtcNow);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var sut = CreateSut(serviceOrderRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_SkipsOrder_WhenMissingFinishedAt()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Alignment", 1.0);
        var estimate = new Estimate(Guid.NewGuid(), "EST-006", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estimate,
            executionStartedAt: DateTime.UtcNow.AddHours(-1), finishedAt: null);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var sut = CreateSut(serviceOrderRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_SkipsOrder_WhenNoEstimateId()
    {
        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished,
            estimate: null,
            executionStartedAt: DateTime.UtcNow.AddHours(-1),
            finishedAt: DateTime.UtcNow);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var sut = CreateSut(serviceOrderRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_SkipsOrder_WhenEstimateNavigationIsNull()
    {
        var so = new ServiceOrder(Guid.NewGuid(), "SO-NOEST", Guid.NewGuid(), Guid.NewGuid());
        SetProperty(so, "Status", ServiceOrderStatus.Finished);
        SetProperty(so, "EstimateId", (Guid?)Guid.NewGuid());
        SetProperty(so, "ExecutionStartedAt", (DateTime?)DateTime.UtcNow.AddHours(-1));
        SetProperty(so, "FinishedAt", (DateTime?)DateTime.UtcNow);
        // Estimate navigation property intentionally left null

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var sut = CreateSut(serviceOrderRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_SkipsServiceItem_WhenServiceIdFilterDoesNotMatch()
    {
        var targetServiceId = Guid.NewGuid();
        var otherServiceId = Guid.NewGuid();
        var targetService = CreateService(targetServiceId, "Paint Job", 8.0);
        var otherService = CreateService(otherServiceId, "Waxing", 2.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-007", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(otherService, 1); // only otherService in estimate

        var start = DateTime.UtcNow.AddHours(-3);
        var end = start.AddHours(2);
        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estimate, start, end);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(targetServiceId).Returns(Task.FromResult(targetService));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetServiceMonitoringAsync(targetServiceId);

        result.ExecutionCount.ShouldBe(0);
        result.AverageRealExecutionTimeInHours.ShouldBe(0);
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_HandlesZeroEstimatedTime_WithoutDivisionByZero()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Quick Check", estimatedTime: 0.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-008", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var start = DateTime.UtcNow.AddHours(-1);
        var end = start.AddMinutes(30);
        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estimate, start, end);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.Count.ShouldBe(1);
        result.Services[0].VariancePercentage.ShouldBe(0);
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_CalculatesMinMaxAndAverage_ForMultipleOrders()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Full Service", estimatedTime: 3.0);

        var est1 = new Estimate(Guid.NewGuid(), "EST-009", Guid.NewGuid(), Guid.NewGuid());
        est1.AddServiceItem(service, 1);
        var est2 = new Estimate(Guid.NewGuid(), "EST-010", Guid.NewGuid(), Guid.NewGuid());
        est2.AddServiceItem(service, 1);
        var est3 = new Estimate(Guid.NewGuid(), "EST-011", Guid.NewGuid(), Guid.NewGuid());
        est3.AddServiceItem(service, 1);

        var now = DateTime.UtcNow;
        var so1 = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, est1, now.AddHours(-2), now.AddHours(-1));      // 1h
        var so2 = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, est2, now.AddHours(-7), now.AddHours(-5));      // 2h
        var so3 = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Delivered, est3, now.AddHours(-14), now.AddHours(-10));   // 4h

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so1, so2, so3 }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so1.Id).Returns(Task.FromResult(so1));
        serviceOrderRepo.GetWithDetailsAsync(so2.Id).Returns(Task.FromResult(so2));
        serviceOrderRepo.GetWithDetailsAsync(so3.Id).Returns(Task.FromResult(so3));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        var monitoring = result.Services[0];
        monitoring.ExecutionCount.ShouldBe(3);
        monitoring.AverageRealExecutionTimeInHours.ShouldBe((1.0 + 2.0 + 4.0) / 3, tolerance: 0.01);
        monitoring.MinExecutionTimeInHours?.ShouldBe(1.0, tolerance: 0.01);
        monitoring.MaxExecutionTimeInHours?.ShouldBe(4.0, tolerance: 0.01);
    }

    [Fact]
    public async Task GetServiceMonitoringDataAsync_OrdersResults_ByExecutionCountDescending()
    {
        var serviceAId = Guid.NewGuid();
        var serviceBId = Guid.NewGuid();
        var serviceA = CreateService(serviceAId, "Service A", 1.0);
        var serviceB = CreateService(serviceBId, "Service B", 1.0);

        var estA1 = new Estimate(Guid.NewGuid(), "EST-A1", Guid.NewGuid(), Guid.NewGuid());
        estA1.AddServiceItem(serviceA, 1);
        var estA2 = new Estimate(Guid.NewGuid(), "EST-A2", Guid.NewGuid(), Guid.NewGuid());
        estA2.AddServiceItem(serviceA, 1);
        var estB1 = new Estimate(Guid.NewGuid(), "EST-B1", Guid.NewGuid(), Guid.NewGuid());
        estB1.AddServiceItem(serviceB, 1);

        var now = DateTime.UtcNow;
        var soA1 = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estA1, now.AddHours(-2), now.AddHours(-1));
        var soA2 = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estA2, now.AddHours(-4), now.AddHours(-3));
        var soB1 = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estB1, now.AddHours(-6), now.AddHours(-5));

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { soA1, soA2, soB1 }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(soA1.Id).Returns(Task.FromResult(soA1));
        serviceOrderRepo.GetWithDetailsAsync(soA2.Id).Returns(Task.FromResult(soA2));
        serviceOrderRepo.GetWithDetailsAsync(soB1.Id).Returns(Task.FromResult(soB1));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceAId).Returns(Task.FromResult(serviceA));
        serviceRepo.GetAsync(serviceBId).Returns(Task.FromResult(serviceB));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.Services.Count.ShouldBe(2);
        result.Services[0].ServiceId.ShouldBe(serviceAId); // 2 executions — first
        result.Services[1].ServiceId.ShouldBe(serviceBId); // 1 execution — second
    }

    // ── BuildOverviewDto ───────────────────────────────────────────────────

    [Fact]
    public async Task BuildOverviewDto_CalculatesAverageAccuracyPercentage_WhenVarianceIsZero()
    {
        var serviceId = Guid.NewGuid();
        var service = CreateService(serviceId, "Battery Replace", estimatedTime: 1.0);

        var estimate = new Estimate(Guid.NewGuid(), "EST-BAT", Guid.NewGuid(), Guid.NewGuid());
        estimate.AddServiceItem(service, 1);

        var start = DateTime.UtcNow.AddHours(-2);
        var end = start.AddHours(1); // exactly as estimated
        var so = CreateServiceOrder(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ServiceOrderStatus.Finished, estimate, start, end);

        var serviceOrderRepo = Substitute.For<IServiceOrderRepository>();
        serviceOrderRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { so }.AsQueryable()));
        serviceOrderRepo.GetWithDetailsAsync(so.Id).Returns(Task.FromResult(so));

        var serviceRepo = Substitute.For<IRepository<Service, Guid>>();
        serviceRepo.GetAsync(serviceId).Returns(Task.FromResult(service));

        var sut = CreateSut(serviceOrderRepo, serviceRepo);

        var result = await sut.GetAllServicesMonitoringAsync();

        result.AverageAccuracyPercentage.ShouldBe(100.0, tolerance: 0.01);
        result.OverallAverageExecutionTimeInHours.ShouldBe(1.0, tolerance: 0.01);
        result.OverallAverageEstimatedTimeInHours.ShouldBe(1.0, tolerance: 0.01);
    }
}
