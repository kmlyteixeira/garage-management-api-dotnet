using System;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Customers;
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
    private readonly IEmailSender emailSender;

    public ServiceOrderAppService(
        IRepository<ServiceOrder, Guid> repository,
        IServiceOrderUpdateMediator serviceOrderUpdateMediator,
        IRepository<Customer, Guid> customerRepository,
        IEmailSender emailSender) : base(repository)
    {
        this.serviceOrderUpdateMediator = serviceOrderUpdateMediator;
        this.customerRepository = customerRepository;
        this.emailSender = emailSender;
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
