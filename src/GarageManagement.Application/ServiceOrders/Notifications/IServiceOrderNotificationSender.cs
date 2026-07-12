using System.Threading.Tasks;
using GarageManagement.Customers;
using GarageManagement.Estimates;

namespace GarageManagement.ServiceOrders.Notifications;

public interface IServiceOrderNotificationSender
{
    Task NotifyStatusChangedAsync(ServiceOrder serviceOrder, Customer customer);

    Task NotifyEstimatePendingApprovalAsync(Estimate estimate, Customer customer);
}
