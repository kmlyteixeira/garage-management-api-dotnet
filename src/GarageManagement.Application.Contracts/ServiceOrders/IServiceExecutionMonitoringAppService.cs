using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GarageManagement.ServiceOrders;

/// <summary>
/// Application service for monitoring service execution times.
/// </summary>
public interface IServiceExecutionMonitoringAppService : IApplicationService
{
    /// <summary>
    /// Gets monitoring data for all services with execution metrics.
    /// </summary>
    /// <returns>Overview with metrics for all services.</returns>
    Task<ServiceExecutionMonitoringOverviewDto> GetAllServicesMonitoringAsync();

    /// <summary>
    /// Gets monitoring data for a specific service.
    /// </summary>
    /// <param name="serviceId">The service ID to monitor.</param>
    /// <returns>Monitoring metrics for the service.</returns>
    Task<ServiceExecutionMonitoringDto> GetServiceMonitoringAsync(Guid serviceId);

    /// <summary>
    /// Gets monitoring data for services by customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>Overview with metrics for services executed for the customer.</returns>
    Task<ServiceExecutionMonitoringOverviewDto> GetCustomerServiceMonitoringAsync(Guid customerId);

    /// <summary>
    /// Gets monitoring data for services by vehicle.
    /// </summary>
    /// <param name="vehicleId">The vehicle ID.</param>
    /// <returns>Overview with metrics for services executed for the vehicle.</returns>
    Task<ServiceExecutionMonitoringOverviewDto> GetVehicleServiceMonitoringAsync(Guid vehicleId);
}
