using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Estimates;
using GarageManagement.Inventories;
using GarageManagement.ServiceOrders;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.Application.Tests.Applications.Estimates;

public class EstimateAppServiceTests
{
    [Fact]
    public async Task ApproveAsync_Should_Throw_When_Estimate_Not_Found()
    {
        var repo = Substitute.For<IRepository<Estimate, Guid>>();
        var readOnly = Substitute.For<Volo.Abp.Domain.Repositories.IReadOnlyRepository<Estimate, Guid>>();
        var partRepo = Substitute.For<IRepository<EstimateProductItem, Guid>>();
        var soRepo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var inventory = Substitute.For<IInventoryAppService>();

        repo.GetAsync(Arg.Any<Guid>(), Arg.Any<bool>()).Returns(Task.FromResult<Estimate>(null!));

        var sut = new EstimateAppService(readOnly, repo, partRepo, soRepo, inventory);

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.ApproveAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RejectAsync_Should_Throw_When_Reason_Empty()
    {
        var repo = Substitute.For<IRepository<Estimate, Guid>>();
        var readOnly = Substitute.For<Volo.Abp.Domain.Repositories.IReadOnlyRepository<Estimate, Guid>>();
        var partRepo = Substitute.For<IRepository<EstimateProductItem, Guid>>();
        var soRepo = Substitute.For<IRepository<ServiceOrder, Guid>>();
        var inventory = Substitute.For<IInventoryAppService>();

        var sut = new EstimateAppService(readOnly, repo, partRepo, soRepo, inventory);

        await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() => sut.RejectAsync(Guid.NewGuid(), new EstimateRejectDto { Reason = " " }));
    }
}
