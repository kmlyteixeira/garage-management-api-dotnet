using System;
using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Estimates;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;

namespace GarageManagement.ServiceOrders.Notifications;

public class EmailServiceOrderNotificationSender : IServiceOrderNotificationSender, ITransientDependency
{
    private readonly IEmailSender emailSender;
    private readonly ILogger<EmailServiceOrderNotificationSender> logger;

    public EmailServiceOrderNotificationSender(IEmailSender emailSender, ILogger<EmailServiceOrderNotificationSender> logger)
    {
        this.emailSender = emailSender;
        this.logger = logger;
    }

    public Task NotifyStatusChangedAsync(ServiceOrder serviceOrder, Customer customer)
    {
        var (subject, body) = BuildStatusChangedMessage(serviceOrder, customer);

        return SendAsync(customer.Email, subject, body, "status atualizado", serviceOrder.Id);
    }

    public Task NotifyEstimatePendingApprovalAsync(Estimate estimate, Customer customer)
    {
        var subject = $"Orçamento {estimate.EstimateNumber} aguardando aprovação";
        var body =
            $"Olá, {customer.Name}!\n\n" +
            $"Seu orçamento {estimate.EstimateNumber} está aguardando sua aprovação.\n" +
            $"Valor total: {estimate.TotalAmount:C}.\n\n" +
            "Acesse o portal para aprovar ou reprovar.";

        return SendAsync(customer.Email, subject, body, "orçamento aguardando aprovação", estimate.Id);
    }

    private static (string Subject, string Body) BuildStatusChangedMessage(ServiceOrder serviceOrder, Customer customer)
    {
        var subject = $"Ordem de serviço {serviceOrder.ServiceOrderNumber} atualizada";
        var statusMessage = serviceOrder.Status switch
        {
            ServiceOrderStatus.WaitingApproval => "está aguardando a aprovação do orçamento.",
            ServiceOrderStatus.InExecution => "entrou em execução.",
            ServiceOrderStatus.Finished => "foi finalizada. Entre em contato com a oficina para combinar retirada/entrega.",
            ServiceOrderStatus.Delivered => "foi entregue. Obrigado por confiar em nossos serviços!",
            _ => "teve seu status atualizado."
        };

        var body =
            $"Olá, {customer.Name}!\n\n" +
            $"Sua ordem de serviço {serviceOrder.ServiceOrderNumber} {statusMessage}\n\n" +
            "Obrigado.";

        return (subject, body);
    }

    private async Task SendAsync(string email, string subject, string body, string eventDescription, Guid entityId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        try
        {
            await emailSender.SendAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao enviar e-mail de notificação ({EventDescription}) para a entidade {EntityId}.", eventDescription, entityId);
        }
    }
}
