using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.ServiceOrders;
using GarageManagement.Vehicles;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Linq;
using Xunit;

namespace GarageManagement.Application.Tests.Applications.ServiceOrders;

public class ServiceOrderAppServiceTests
{
    // Evaluates IAsyncQueryableExecuter calls in-memory so we can test without a real DB
    private static IAsyncQueryableExecuter CreateInMemoryExecuter()
    {
        var executer = Substitute.For<IAsyncQueryableExecuter>();

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
        IRepository<Vehicle, Guid>? vehicleRepo = null)
    {
        var sut = new ServiceOrderAppService(
            repo ?? Substitute.For<IRepository<ServiceOrder, Guid>>(),
            Substitute.For<IServiceOrderUpdateMediator>(),
            customerRepo ?? Substitute.For<IRepository<Customer, Guid>>(),
            vehicleRepo ?? Substitute.For<IRepository<Vehicle, Guid>>(),
            Substitute.For<IEmailSender>());

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
        var emailSender = Substitute.For<IEmailSender>();

        repo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult<ServiceOrder?>(null));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, emailSender);

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.UpdateAsync(Guid.NewGuid(), new ServiceOrderUpdateDto()));
    }

    [Fact]
    public async Task UpdateAsync_Should_Delegate_To_Mediator_When_ServiceOrder_Exists()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var vehicleRepo = Substitute.For<IRepository<Vehicle, Guid>>();
        var emailSender = Substitute.For<IEmailSender>();
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());
        var expected = new ServiceOrderDto();

        repo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));
        mediator.UpdateAsync(serviceOrder, Arg.Any<ServiceOrderUpdateDto>()).Returns(Task.FromResult(expected));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, vehicleRepo, emailSender);

        var result = await sut.UpdateAsync(Guid.NewGuid(), new ServiceOrderUpdateDto());

        result.ShouldBe(expected);
        await mediator.Received(1).UpdateAsync(serviceOrder, Arg.Any<ServiceOrderUpdateDto>());
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
