using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GarageManagement.ServiceOrders.Tests;

public class ServiceOrderUpdateMediatorTests
{
    [Fact]
    public async Task UpdateAsync_Should_Delegate_To_Handler_And_Return_Result()
    {
        var handler = Substitute.For<IServiceOrderUpdateHandler>();
        var mediator = new ServiceOrderUpdateMediator(handler);
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), "SO-1", Guid.NewGuid(), Guid.NewGuid());
        var input = new ServiceOrderUpdateDto();
        var expected = new ServiceOrderDto();

        handler.HandleAsync(serviceOrder, input).Returns(Task.FromResult(expected));

        var result = await mediator.UpdateAsync(serviceOrder, input);

        result.ShouldBe(expected);
        await handler.Received(1).HandleAsync(serviceOrder, input);
    }
}