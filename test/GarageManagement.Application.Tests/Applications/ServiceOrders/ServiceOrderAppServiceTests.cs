using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.ServiceOrders;
using GarageManagement.ServiceOrders.Notifications;
using GarageManagement.Vehicles;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace GarageManagement.Application.Tests.Applications.ServiceOrders;

public class ServiceOrderAppServiceTests
{
    // ObjectMapper is a get-only property on ApplicationService, resolved as
    // LazyServiceProvider.LazyGetService<IObjectMapper>(Func<IServiceProvider,object>), so it must
    // be stubbed via that exact overload on the lazy provider rather than set directly.
    private static void SetObjectMapper(ServiceOrderAppService sut, IObjectMapper objectMapper)
    {
        var lazyProvider = sut.LazyServiceProvider ?? Substitute.For<IAbpLazyServiceProvider>();
        lazyProvider.LazyGetService<IObjectMapper>(Arg.Any<Func<IServiceProvider, object>>()).Returns(objectMapper);
        sut.LazyServiceProvider = lazyProvider;
    }

    // Evaluates IAsyncQueryableExecuter calls in-memory so we can test without a real DB
    private static IAsyncQueryableExecuter CreateInMemoryExecuter()
    {
        var executer = Substitute.For<IAsyncQueryableExecuter>();

        executer
            .ToListAsync(Arg.Any<IQueryable<Customer>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(((IQueryable<Customer>)ci[0]).ToList()));

        executer
            .ToListAsync(Arg.Any<IQueryable<Vehicle>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(((IQueryable<Vehicle>)ci[0]).ToList()));

        executer
            .ToListAsync(Arg.Any<IQueryable<ServiceOrder>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(((IQueryable<ServiceOrder>)ci[0]).ToList()));

        executer
            .FirstOrDefaultAsync(Arg.Any<IQueryable<Customer>>(), Arg.Any<Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(
                ((IQueryable<Customer>)ci[0]).FirstOrDefault((Expression<Func<Customer, bool>>)ci[1])));

        executer
            .FirstOrDefaultAsync(Arg.Any<IQueryable<Vehicle>>(), Arg.Any<Expression<Func<Vehicle, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(
                ((IQueryable<Vehicle>)ci[0]).FirstOrDefault((Expression<Func<Vehicle, bool>>)ci[1])));

        executer
            .FirstOrDefaultAsync(Arg.Any<IQueryable<ServiceOrder>>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(
                ((IQueryable<ServiceOrder>)ci[0]).FirstOrDefault()));

        return executer;
    }

    private static ServiceOrderAppService CreateSut(
        IRepository<ServiceOrder, Guid>? repo = null,
        IRepository<Customer, Guid>? customerRepo = null,
        IRepository<Vehicle, Guid>? vehicleRepo = null,
        IServiceOrderUpdateMediator? mediator = null,
        IServiceOrderNotificationSender? notificationSender = null)
    {
        var sut = new ServiceOrderAppService(
            repo ?? Substitute.For<IRepository<ServiceOrder, Guid>>(),
            mediator ?? Substitute.For<IServiceOrderUpdateMediator>(),
            customerRepo ?? Substitute.For<IRepository<Customer, Guid>>(),
            vehicleRepo ?? Substitute.For<IRepository<Vehicle, Guid>>(),
            notificationSender ?? Substitute.For<IServiceOrderNotificationSender>());

        // AsyncExecuter is protected; it's resolved from LazyServiceProvider which is public.
        // The executer must be created before .Returns() to avoid NSubstitute call-recording interference.
        var executer = CreateInMemoryExecuter();
        var lazyProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyProvider.LazyGetRequiredService<IAsyncQueryableExecuter>().Returns(executer);
        sut.LazyServiceProvider = lazyProvider;

        return sut;
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_ServiceOrder_Is_Not_Found()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var notificationSender = Substitute.For<IServiceOrderNotificationSender>();

        repo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult<ServiceOrder?>(null));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, notificationSender);

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.UpdateAsync(Guid.NewGuid(), new ServiceOrderUpdateDto()));
    }

    [Fact]
    public async Task UpdateAsync_Should_Delegate_To_Mediator_When_ServiceOrder_Exists()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var notificationSender = Substitute.For<IServiceOrderNotificationSender>();
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());
        var expected = new ServiceOrderDto();

        repo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));
        mediator.UpdateAsync(serviceOrder, Arg.Any<ServiceOrderUpdateDto>()).Returns(Task.FromResult(expected));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, notificationSender);

        var result = await sut.UpdateAsync(Guid.NewGuid(), new ServiceOrderUpdateDto());

        result.ShouldBe(expected);
        await mediator.Received(1).UpdateAsync(serviceOrder, Arg.Any<ServiceOrderUpdateDto>());
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Throw_When_Status_Is_Missing()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var notificationSender = Substitute.For<IServiceOrderNotificationSender>();
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-2", Guid.NewGuid(), Guid.NewGuid());

        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, notificationSender);

        await Should.ThrowAsync<UserFriendlyException>(() =>
            sut.UpdateStatusAsync(Guid.NewGuid(), new ServiceOrderUpdateStatusDto { Status = null }));
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Notify_Customer_When_ServiceOrder_Is_Finished()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var notificationSender = Substitute.For<IServiceOrderNotificationSender>();

        var customer = new Customer("João", "joao@email.com", "11999999999", new Document("12345678901"));
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-3", customer.Id, Guid.NewGuid());
        serviceOrder.StartDiagnosis();
        serviceOrder.AssociateEstimate(Guid.NewGuid());
        serviceOrder.WaitApproval();
        serviceOrder.WaitExecution();
        serviceOrder.StartExecution();

        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));
        customerRepo.GetAsync(serviceOrder.CustomerId).Returns(Task.FromResult(customer));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, notificationSender);
        var objectMapper = Substitute.For<IObjectMapper>();
        objectMapper.Map<ServiceOrder, ServiceOrderDto>(Arg.Any<ServiceOrder>())
            .Returns(ci => new ServiceOrderDto { Status = ((ServiceOrder)ci[0]).Status });
        SetObjectMapper(sut, objectMapper);

        var result = await sut.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Finished });

        result.Status.ShouldBe(ServiceOrderStatus.Finished);
        serviceOrder.Status.ShouldBe(ServiceOrderStatus.Finished);
        await repo.Received(1).UpdateAsync(serviceOrder, autoSave: true);
        await notificationSender.Received(1).NotifyStatusChangedAsync(serviceOrder, customer);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.InExecution)]
    [InlineData(ServiceOrderStatus.Delivered)]
    public async Task UpdateStatusAsync_Should_Notify_Customer_On_Other_Relevant_Transitions(ServiceOrderStatus targetStatus)
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var notificationSender = Substitute.For<IServiceOrderNotificationSender>();

        var customer = new Customer("Maria", "maria@email.com", "11988888888", new Document("98765432100"));
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-4", customer.Id, Guid.NewGuid());
        serviceOrder.StartDiagnosis();
        serviceOrder.AssociateEstimate(Guid.NewGuid());
        serviceOrder.WaitApproval();
        serviceOrder.WaitExecution();
        if (targetStatus == ServiceOrderStatus.Delivered)
        {
            // Delivered can only be reached from Finished, which in turn requires InExecution.
            serviceOrder.StartExecution();
            serviceOrder.Finish();
        }

        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));
        customerRepo.GetAsync(serviceOrder.CustomerId).Returns(Task.FromResult(customer));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, notificationSender);
        var objectMapper = Substitute.For<IObjectMapper>();
        objectMapper.Map<ServiceOrder, ServiceOrderDto>(Arg.Any<ServiceOrder>()).Returns(new ServiceOrderDto());
        SetObjectMapper(sut, objectMapper);

        await sut.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = targetStatus });

        serviceOrder.Status.ShouldBe(targetStatus);
        await notificationSender.Received(1).NotifyStatusChangedAsync(serviceOrder, customer);
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Not_Notify_Customer_On_Non_Relevant_Transitions()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var notificationSender = Substitute.For<IServiceOrderNotificationSender>();

        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-5", Guid.NewGuid(), Guid.NewGuid());

        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, notificationSender);
        var objectMapper = Substitute.For<IObjectMapper>();
        objectMapper.Map<ServiceOrder, ServiceOrderDto>(Arg.Any<ServiceOrder>()).Returns(new ServiceOrderDto());
        SetObjectMapper(sut, objectMapper);

        await sut.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.InDiagnosis });

        serviceOrder.Status.ShouldBe(ServiceOrderStatus.InDiagnosis);
        await customerRepo.DidNotReceive().GetAsync(Arg.Any<Guid>());
        await notificationSender.DidNotReceive().NotifyStatusChangedAsync(Arg.Any<ServiceOrder>(), Arg.Any<Customer>());
    }

    [Fact]
    public async Task OpenAsync_Should_Create_ServiceOrder_Start_Diagnosis_And_Delegate_Items_To_Mediator()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var expected = new ServiceOrderDto();

        mediator
            .UpdateAsync(Arg.Is<ServiceOrder>(so => so.Status == ServiceOrderStatus.InDiagnosis), Arg.Any<ServiceOrderUpdateDto>())
            .Returns(Task.FromResult(expected));

        var sut = CreateSut(repo: repo, mediator: mediator);

        var input = new ServiceOrderOpenDto
        {
            ServiceOrderNumber = "OS-OPEN-1",
            CustomerId = customerId,
            VehicleId = vehicleId,
            ServiceItems = new List<ServiceOrderServiceItemCreateDto> { new() { ServiceId = Guid.NewGuid(), Quantity = 1 } },
            PartItems = new List<ServiceOrderProductItemCreateDto> { new() { ProductId = Guid.NewGuid(), Quantity = 2 } }
        };

        var result = await sut.OpenAsync(input);

        result.ShouldBe(expected);
        await repo.Received(1).InsertAsync(Arg.Is<ServiceOrder>(so =>
            so.ServiceOrderNumber == "OS-OPEN-1" && so.CustomerId == customerId && so.VehicleId == vehicleId), autoSave: true);
        await repo.Received(1).UpdateAsync(Arg.Is<ServiceOrder>(so => so.Status == ServiceOrderStatus.InDiagnosis), autoSave: true);
        await mediator.Received(1).UpdateAsync(
            Arg.Any<ServiceOrder>(),
            Arg.Is<ServiceOrderUpdateDto>(dto => dto.ServiceItems == input.ServiceItems && dto.PartItems == input.PartItems));
    }

    [Fact]
    public async Task GetPublicStatusAsync_Should_Throw_When_Document_Is_Empty()
    {
        var sut = CreateSut();

        var ex = await Should.ThrowAsync<UserFriendlyException>(() =>
            sut.GetPublicStatusAsync(new ServiceOrderPublicStatusRequestDto
            {
                Document = "",
                LicensePlate = "ABC1234"
            }));

        ex.Message.ShouldContain("CPF/CNPJ");
    }

    [Fact]
    public async Task GetPublicStatusAsync_Should_Throw_When_LicensePlate_Is_Empty()
    {
        var sut = CreateSut();

        var ex = await Should.ThrowAsync<UserFriendlyException>(() =>
            sut.GetPublicStatusAsync(new ServiceOrderPublicStatusRequestDto
            {
                Document = "12345678901",
                LicensePlate = ""
            }));

        ex.Message.ShouldContain("CPF/CNPJ");
    }

    [Fact]
    public async Task GetPublicStatusAsync_Should_Throw_When_Customer_Not_Found()
    {
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        customerRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Customer>().AsQueryable()));

        var sut = CreateSut(customerRepo: customerRepo);

        var ex = await Should.ThrowAsync<UserFriendlyException>(() =>
            sut.GetPublicStatusAsync(new ServiceOrderPublicStatusRequestDto
            {
                Document = "12345678901",
                LicensePlate = "ABC1234"
            }));

        ex.Message.ShouldContain("Nenhuma ordem de serviço encontrada");
    }

    [Fact]
    public async Task GetPublicStatusAsync_Should_Throw_When_Vehicle_Not_Found()
    {
        var customer = new Customer("João", "joao@email.com", "11999999999", new Document("12345678901"));
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        customerRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Customer> { customer }.AsQueryable()));

        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        vehicleRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Vehicle>().AsQueryable()));

        var sut = CreateSut(customerRepo: customerRepo, vehicleRepo: vehicleRepo);

        var ex = await Should.ThrowAsync<UserFriendlyException>(() =>
            sut.GetPublicStatusAsync(new ServiceOrderPublicStatusRequestDto
            {
                Document = "12345678901",
                LicensePlate = "ABC1234"
            }));

        ex.Message.ShouldContain("Nenhuma ordem de serviço encontrada");
    }

    [Fact]
    public async Task GetPublicStatusAsync_Should_Throw_When_ServiceOrder_Not_Found()
    {
        var customer = new Customer("João", "joao@email.com", "11999999999", new Document("12345678901"));
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        customerRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Customer> { customer }.AsQueryable()));

        var vehicle = new Vehicle("Toyota", "Corolla", 2020, "ABC-1234");
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        vehicleRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Vehicle> { vehicle }.AsQueryable()));

        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        repo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder>().AsQueryable()));

        var sut = CreateSut(repo: repo, customerRepo: customerRepo, vehicleRepo: vehicleRepo);

        var ex = await Should.ThrowAsync<UserFriendlyException>(() =>
            sut.GetPublicStatusAsync(new ServiceOrderPublicStatusRequestDto
            {
                Document = "12345678901",
                LicensePlate = "ABC-1234"
            }));

        ex.Message.ShouldContain("Nenhuma ordem de serviço encontrada");
    }

    [Fact]
    public async Task GetPublicStatusAsync_Should_Return_PublicStatus_When_All_Data_Found()
    {
        var customer = new Customer("João", "joao@email.com", "11999999999", new Document("12345678901"));
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        customerRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Customer> { customer }.AsQueryable()));

        var vehicle = new Vehicle("Toyota", "Corolla", 2020, "ABC-1234");
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        vehicleRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<Vehicle> { vehicle }.AsQueryable()));

        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-001", customer.Id, vehicle.Id);
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        repo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<ServiceOrder> { serviceOrder }.AsQueryable()));

        var sut = CreateSut(repo: repo, customerRepo: customerRepo, vehicleRepo: vehicleRepo);

        var result = await sut.GetPublicStatusAsync(new ServiceOrderPublicStatusRequestDto
        {
            Document = "123.456.789-01",
            LicensePlate = "ABC-1234"
        });

        result.ShouldNotBeNull();
        result.ServiceOrderNumber.ShouldBe("SO-001");
        result.Status.ShouldBe(ServiceOrderStatus.Received);
        result.CreatedAt.ShouldBe(serviceOrder.CreatedAt);
    }
}
