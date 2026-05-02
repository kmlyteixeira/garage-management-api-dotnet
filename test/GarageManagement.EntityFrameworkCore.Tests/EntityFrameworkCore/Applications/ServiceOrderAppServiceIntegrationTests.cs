using System;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.EntityFrameworkCore.TestDoubles;
using GarageManagement.Estimates;
using GarageManagement.ServiceOrders;
using GarageManagement.Vehicles;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace GarageManagement.EntityFrameworkCore.Applications;

[Collection(GarageManagementTestConsts.CollectionDefinitionName)]
public class ServiceOrderAppServiceIntegrationTests : GarageManagementEntityFrameworkCoreTestBase
{
    private readonly ServiceOrderAppService serviceOrderAppService;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Vehicle, Guid> vehicleRepository;
    private readonly IRepository<Estimate, Guid> estimateRepository;
    private readonly IRepository<ServiceOrder, Guid> serviceOrderRepository;
    private readonly TestEmailSender testEmailSender;

    public ServiceOrderAppServiceIntegrationTests()
    {
        serviceOrderAppService = GetRequiredService<ServiceOrderAppService>();
        customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        vehicleRepository = GetRequiredService<IRepository<Vehicle, Guid>>();
        estimateRepository = GetRequiredService<IRepository<Estimate, Guid>>();
        serviceOrderRepository = GetRequiredService<IRepository<ServiceOrder, Guid>>();
        testEmailSender = GetRequiredService<TestEmailSender>();
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Move_Received_To_InDiagnosis()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("VW", "Gol", 2019, $"GHI{suffix}");
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", Guid.Empty, Guid.Empty);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            var result = await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id,
                new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.InDiagnosis });

            var updated = await serviceOrderRepository.GetAsync(serviceOrder.Id);

            result.Status.ShouldBe(ServiceOrderStatus.InDiagnosis);
            updated.Status.ShouldBe(ServiceOrderStatus.InDiagnosis);
            updated.DiagnosisStartedAt.ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task UpdateStatusAsync_With_Null_Status_Should_Throw_UserFriendlyException()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Fiat", "Argo", 2022, $"JKL{suffix}");
        var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", Guid.Empty, Guid.Empty);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await Should.ThrowAsync<UserFriendlyException>(() => serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id,
                new ServiceOrderUpdateStatusDto { Status = null }));
        });
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Progress_To_Closed_And_Send_Notification_When_Finished()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Toyota", "Yaris", 2023, $"MNO{suffix}");

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);
            await estimateRepository.InsertAsync(estimate, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.AssociateEstimate(estimate.Id);
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.InDiagnosis });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.WaitingApproval });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.WaitingExecution });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.InExecution });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Finished });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Delivered });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Closed });

            var updated = await serviceOrderRepository.GetAsync(serviceOrder.Id);

            updated.Status.ShouldBe(ServiceOrderStatus.Closed);
            updated.DiagnosisStartedAt.ShouldNotBeNull();
            updated.ExecutionStartedAt.ShouldNotBeNull();
            updated.FinishedAt.ShouldNotBeNull();
            updated.DeliveredAt.ShouldNotBeNull();
            updated.ClosedAt.ShouldNotBeNull();

            testEmailSender.SentEmails.Count.ShouldBe(1);
            testEmailSender.SentEmails.ShouldContain(message =>
                message.To == customer.Email &&
                message.Subject.Contains(serviceOrder.ServiceOrderNumber));
        });
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Ignore_Email_Sender_Failures_When_Finishing()
    {
        testEmailSender.Clear();
        testEmailSender.ThrowOnSend = true;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Honda", "City", 2024, $"QRS{suffix}");

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);
            await estimateRepository.InsertAsync(estimate, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.AssociateEstimate(estimate.Id);
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.InDiagnosis });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.WaitingApproval });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.WaitingExecution });
            await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.InExecution });

            var result = await serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id, new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Finished });

            var updated = await serviceOrderRepository.GetAsync(serviceOrder.Id);

            result.Status.ShouldBe(ServiceOrderStatus.Finished);
            updated.Status.ShouldBe(ServiceOrderStatus.Finished);
            testEmailSender.SentEmails.Count.ShouldBe(0);
        });

        testEmailSender.ThrowOnSend = false;
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Throw_When_Trying_To_Finish_Directly_From_Received()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Honda", "Fit", 2018, $"STU{suffix}");

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await Should.ThrowAsync<InvalidOperationException>(() =>
                serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id,
                    new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Finished }));

            var persisted = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            persisted.Status.ShouldBe(ServiceOrderStatus.Received);
            testEmailSender.SentEmails.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Throw_When_Closing_Before_Delivered()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Hyundai", "HB20", 2021, $"VWX{suffix}");
        var estimate = new Estimate(Guid.NewGuid(), $"EST-{suffix}", customer.Id, vehicle.Id);

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);
            await estimateRepository.InsertAsync(estimate, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.AssociateEstimate(estimate.Id);
            serviceOrder.StartDiagnosis();
            serviceOrder.WaitApproval();
            serviceOrder.WaitExecution();
            serviceOrder.StartExecution();
            serviceOrder.Finish();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await Should.ThrowAsync<InvalidOperationException>(() =>
                serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id,
                    new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.Closed }));

            var persisted = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            persisted.Status.ShouldBe(ServiceOrderStatus.Finished);
            testEmailSender.SentEmails.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Throw_When_WaitingApproval_Without_Estimate()
    {
        testEmailSender.Clear();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer($"Cliente {suffix}", $"{suffix}@mail.com", "11999999999", new Document($"DOC{suffix}"));
        var vehicle = new Vehicle("Nissan", "Versa", 2020, $"YZA{suffix}");

        await WithUnitOfWorkAsync(async () =>
        {
            await customerRepository.InsertAsync(customer, autoSave: true);
            await vehicleRepository.InsertAsync(vehicle, autoSave: true);

            var serviceOrder = new ServiceOrder(Guid.NewGuid(), $"OS-{suffix}", customer.Id, vehicle.Id);
            serviceOrder.StartDiagnosis();
            await serviceOrderRepository.InsertAsync(serviceOrder, autoSave: true);

            await Should.ThrowAsync<InvalidOperationException>(() =>
                serviceOrderAppService.UpdateStatusAsync(serviceOrder.Id,
                    new ServiceOrderUpdateStatusDto { Status = ServiceOrderStatus.WaitingApproval }));

            var persisted = await serviceOrderRepository.GetAsync(serviceOrder.Id);
            persisted.Status.ShouldBe(ServiceOrderStatus.InDiagnosis);
        });
    }
}