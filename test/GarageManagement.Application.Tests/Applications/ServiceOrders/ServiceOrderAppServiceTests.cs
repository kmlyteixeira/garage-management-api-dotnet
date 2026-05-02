using System;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.ServiceOrders;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Xunit;

namespace GarageManagement.Application.Tests.Applications.ServiceOrders;

public class ServiceOrderAppServiceTests
{
    [Fact]
    public async Task UpdateAsync_Should_Throw_When_ServiceOrder_Is_Not_Found()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var emailSender = Substitute.For<IEmailSender>();

        repo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult<ServiceOrder?>(null));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, emailSender);

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.UpdateAsync(Guid.NewGuid(), new ServiceOrderUpdateDto()));
    }

    [Fact]
    public async Task UpdateAsync_Should_Delegate_To_Mediator_When_ServiceOrder_Exists()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var emailSender = Substitute.For<IEmailSender>();
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());
        var expected = new ServiceOrderDto();

        repo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult<ServiceOrder?>(serviceOrder));
        mediator.UpdateAsync(serviceOrder, Arg.Any<ServiceOrderUpdateDto>()).Returns(Task.FromResult(expected));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, emailSender);

        var result = await sut.UpdateAsync(Guid.NewGuid(), new ServiceOrderUpdateDto());

        result.ShouldBe(expected);
        await mediator.Received(1).UpdateAsync(serviceOrder, Arg.Any<ServiceOrderUpdateDto>());
    }

}
