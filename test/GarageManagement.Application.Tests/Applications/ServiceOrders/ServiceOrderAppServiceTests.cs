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
    public async Task UpdateStatusAsync_Should_Throw_When_Status_Null()
    {
        var repo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var mediator = Substitute.For<IServiceOrderUpdateMediator>();
        var customerRepo = Substitute.For<IRepository<Customer, Guid>>();
        var emailSender = Substitute.For<IEmailSender>();

        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());
        serviceOrder.StartDiagnosis();

        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(Task.FromResult(serviceOrder));

        var sut = new ServiceOrderAppService(repo, mediator, customerRepo, emailSender);

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.UpdateStatusAsync(Guid.NewGuid(), new ServiceOrderUpdateStatusDto { Status = (ServiceOrderStatus?)null }));
    }
}
