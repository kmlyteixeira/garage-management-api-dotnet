using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Permissions;
using GarageManagement.ServiceOrders.Notifications;
using GarageManagement.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderAppService :
    CrudAppService<ServiceOrder, ServiceOrderDto, Guid, ServiceOrderGetListInputDto, ServiceOrderCreateDto, ServiceOrderUpdateDto>,
    IServiceOrderAppService
{
    private static readonly HashSet<ServiceOrderStatus> CustomerNotifiableStatuses = new()
    {
        ServiceOrderStatus.WaitingApproval,
        ServiceOrderStatus.InExecution,
        ServiceOrderStatus.Finished,
        ServiceOrderStatus.Delivered
    };

    private readonly IServiceOrderUpdateMediator serviceOrderUpdateMediator;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Vehicle, Guid> vehicleRepository;
    private readonly IServiceOrderNotificationSender notificationSender;

    public ServiceOrderAppService(
        IRepository<ServiceOrder, Guid> repository,
        IServiceOrderUpdateMediator serviceOrderUpdateMediator,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vehicle, Guid> vehicleRepository,
        IServiceOrderNotificationSender notificationSender) : base(repository)
    {
        this.serviceOrderUpdateMediator = serviceOrderUpdateMediator;
        this.customerRepository = customerRepository;
        this.vehicleRepository = vehicleRepository;
        this.notificationSender = notificationSender;

        GetPolicyName = GarageManagementPermissions.ServiceOrders.Default;
        GetListPolicyName = GarageManagementPermissions.ServiceOrders.Default;
        CreatePolicyName = GarageManagementPermissions.ServiceOrders.Create;
        UpdatePolicyName = GarageManagementPermissions.ServiceOrders.Edit;
        DeletePolicyName = GarageManagementPermissions.ServiceOrders.Delete;
    }

    public override async Task<ServiceOrderDto> CreateAsync(ServiceOrderCreateDto input)
    {
        var entity = new ServiceOrder(GuidGenerator.Create(), input.ServiceOrderNumber, input.CustomerId, input.VehicleId);

        await Repository.InsertAsync(entity, autoSave: true);

        return ObjectMapper.Map<ServiceOrder, ServiceOrderDto>(entity);
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.Create)]
    public async Task<ServiceOrderDto> OpenAsync(ServiceOrderOpenDto input)
    {
        var entity = new ServiceOrder(GuidGenerator.Create(), input.ServiceOrderNumber, input.CustomerId, input.VehicleId);

        await Repository.InsertAsync(entity, autoSave: true);

        entity.StartDiagnosis();
        await Repository.UpdateAsync(entity, autoSave: true);

        var updateDto = new ServiceOrderUpdateDto
        {
            ServiceItems = input.ServiceItems,
            PartItems = input.PartItems
        };

        return await serviceOrderUpdateMediator.UpdateAsync(entity, updateDto);
    }

    public override async Task<ServiceOrderDto> UpdateAsync(Guid id, ServiceOrderUpdateDto input)
    {
        var serviceOrder = await Repository.GetAsync(id)
            ?? throw new UserFriendlyException("Ordem de serviço não encontrada.");

        return await serviceOrderUpdateMediator.UpdateAsync(serviceOrder, input);
    }

    protected override async Task<IQueryable<ServiceOrder>> CreateFilteredQueryAsync(ServiceOrderGetListInputDto input)
    {
        var query = await base.CreateFilteredQueryAsync(input);

        if (input.EstimateId.HasValue)
        {
            query = query.Where(so => so.EstimateId == input.EstimateId);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(so => so.Status == input.Status);
        }
        else if (!input.IncludeFinalized)
        {
            query = query.Where(so => so.Status != ServiceOrderStatus.Finished && so.Status != ServiceOrderStatus.Delivered);
        }

        return query;
    }

    protected override IQueryable<ServiceOrder> ApplySorting(IQueryable<ServiceOrder> query, ServiceOrderGetListInputDto input)
    {
        return query
            .OrderBy(so => so.Status == ServiceOrderStatus.InExecution ? 0
                : so.Status == ServiceOrderStatus.WaitingApproval ? 1
                : so.Status == ServiceOrderStatus.InDiagnosis ? 2
                : so.Status == ServiceOrderStatus.Received ? 3
                : 4)
            .ThenBy(so => so.CreatedAt);
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.UpdateStatus)]
    public async Task<ServiceOrderDto> UpdateStatusAsync(Guid id, ServiceOrderUpdateStatusDto input)
    {
        var serviceOrder = await Repository.GetAsync(id, includeDetails: true)
            ?? throw new UserFriendlyException("Ordem de serviço não encontrada.");

        if (!input.Status.HasValue)
        {
            throw new UserFriendlyException("Informe um status válido para a ordem de serviço.");
        }

        serviceOrder.ChangeStatus(input.Status.Value);

        await Repository.UpdateAsync(serviceOrder, autoSave: true);

        if (CustomerNotifiableStatuses.Contains(serviceOrder.Status))
        {
            var customer = await customerRepository.GetAsync(serviceOrder.CustomerId);
            await notificationSender.NotifyStatusChangedAsync(serviceOrder, customer);
        }

        return ObjectMapper.Map<ServiceOrder, ServiceOrderDto>(serviceOrder);
    }

    [AllowAnonymous]
    public async Task<ServiceOrderPublicStatusDto> GetPublicStatusAsync(ServiceOrderPublicStatusRequestDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Document) || string.IsNullOrWhiteSpace(input.LicensePlate))
        {
            throw new UserFriendlyException("Informe CPF/CNPJ e placa do veículo.");
        }

        input.Normalize();

        var normalizedDocument = input.Document;
        var normalizedPlate = input.LicensePlate;

        var customersQuery = await customerRepository.GetQueryableAsync();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(
            customersQuery,
            c => c.Document.Value == normalizedDocument);

        if (customer is null)
        {
            throw new UserFriendlyException("Nenhuma ordem de serviço encontrada para os dados informados.");
        }

        var vehiclesQuery = await vehicleRepository.GetQueryableAsync();
        var vehicle = await AsyncExecuter.FirstOrDefaultAsync(
            vehiclesQuery,
            v => v.LicensePlate.Replace("-", string.Empty).Replace(" ", string.Empty).ToUpper() == normalizedPlate);

        if (vehicle is null)
        {
            throw new UserFriendlyException("Nenhuma ordem de serviço encontrada para os dados informados.");
        }

        var serviceOrdersQuery = await Repository.GetQueryableAsync();
        var serviceOrder = await AsyncExecuter.FirstOrDefaultAsync(
            serviceOrdersQuery
                .Where(so => so.CustomerId == customer.Id && so.VehicleId == vehicle.Id)
                .OrderByDescending(so => so.CreatedAt));

        if (serviceOrder is null)
        {
            throw new UserFriendlyException("Nenhuma ordem de serviço encontrada para os dados informados.");
        }

        return new ServiceOrderPublicStatusDto
        {
            ServiceOrderNumber = serviceOrder.ServiceOrderNumber,
            Status = serviceOrder.Status,
            CreatedAt = serviceOrder.CreatedAt
        };
    }
}
