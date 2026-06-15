using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Permissions;
using GarageManagement.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;

namespace GarageManagement.ServiceOrders;

public class ServiceOrderAppService :
    CrudAppService<ServiceOrder, ServiceOrderDto, Guid, ServiceOrderGetListInputDto, ServiceOrderCreateDto, ServiceOrderUpdateDto>,
    IServiceOrderAppService
{
    private readonly IServiceOrderUpdateMediator serviceOrderUpdateMediator;
    private readonly IRepository<Customer, Guid> customerRepository;
    private readonly IRepository<Vehicle, Guid> vehicleRepository;
    private readonly IEmailSender emailSender;

    public ServiceOrderAppService(
        IRepository<ServiceOrder, Guid> repository,
        IServiceOrderUpdateMediator serviceOrderUpdateMediator,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vehicle, Guid> vehicleRepository,
        IEmailSender emailSender) : base(repository)
    {
        this.serviceOrderUpdateMediator = serviceOrderUpdateMediator;
        this.customerRepository = customerRepository;
        this.vehicleRepository = vehicleRepository;
        this.emailSender = emailSender;

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

    public override async Task<ServiceOrderDto> UpdateAsync(Guid id, ServiceOrderUpdateDto input)
    {
        var serviceOrder = await Repository.GetAsync(id)
            ?? throw new UserFriendlyException("Ordem de serviço não encontrada.");

        return await serviceOrderUpdateMediator.UpdateAsync(serviceOrder, input);
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.UpdateStatus)]
    public async Task<ServiceOrderDto> UpdateStatusAsync(Guid id, ServiceOrderUpdateStatusDto input)
    {
        var serviceOrder = await Repository.GetAsync(id, includeDetails: true)
            ?? throw new UserFriendlyException("Ordem de serviço não encontrada.");

        var previousStatus = serviceOrder.Status;

        if (!input.Status.HasValue)
        {
            throw new UserFriendlyException("Informe um status válido para a ordem de serviço.");
        }

        serviceOrder.ChangeStatus(input.Status.Value);

        await Repository.UpdateAsync(serviceOrder, autoSave: true);

        if (previousStatus != ServiceOrderStatus.Finished && serviceOrder.Status == ServiceOrderStatus.Finished)
        {
            await NotifyCustomerOrderFinishedAsync(serviceOrder);
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
        var customersList = await AsyncExecuter.ToListAsync(customersQuery);
        var customer = customersList.FirstOrDefault(c => c.Document.Value.Equals(normalizedDocument, StringComparison.OrdinalIgnoreCase));

        if (customer is null)
        {
            throw new UserFriendlyException("Nenhuma ordem de serviço encontrada para os dados informados.");
        }

        var vehiclesQuery = await vehicleRepository.GetQueryableAsync();
        var vehiclesList = await AsyncExecuter.ToListAsync(vehiclesQuery);
        var vehicle = vehiclesList.FirstOrDefault(v =>
            v.LicensePlate
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .ToUpper() == normalizedPlate);

        if (vehicle is null)
        {
            throw new UserFriendlyException("Nenhuma ordem de serviço encontrada para os dados informados.");
        }

        // Fetch service orders and filter in memory
        var serviceOrdersQuery = await Repository.GetQueryableAsync();
        var serviceOrdersList = await AsyncExecuter.ToListAsync(serviceOrdersQuery);
        var serviceOrder = serviceOrdersList
            .Where(so => so.CustomerId == customer.Id && so.VehicleId == vehicle.Id)
            .OrderByDescending(so => so.CreatedAt)
            .FirstOrDefault();

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

    private async Task NotifyCustomerOrderFinishedAsync(ServiceOrder serviceOrder)
    {
        var customer = await customerRepository.GetAsync(serviceOrder.CustomerId);
        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            return;
        }

        var subject = $"Ordem de serviço {serviceOrder.ServiceOrderNumber} finalizada";
        var body =
            $"Olá, {customer.Name}!\n\n" +
            $"Sua ordem de serviço {serviceOrder.ServiceOrderNumber} foi finalizada.\n" +
            "Entre em contato com a oficina para combinar retirada/entrega.\n\n" +
            "Obrigado.";

        try
        {
            await emailSender.SendAsync(customer.Email, subject, body);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Falha ao enviar e-mail de OS finalizada para a ordem {ServiceOrderId}.", serviceOrder.Id);
        }
    }
}
