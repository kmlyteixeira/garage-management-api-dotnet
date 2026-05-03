using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GarageManagement.Estimates;
using GarageManagement.Permissions;
using GarageManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.ServiceOrders;

[Authorize(GarageManagementPermissions.ServiceOrders.Default)]
public class ServiceExecutionMonitoringAppService : ApplicationService, IServiceExecutionMonitoringAppService
{
    private readonly IServiceOrderRepository serviceOrderRepository;
    private readonly IRepository<Service, Guid> serviceRepository;

    public ServiceExecutionMonitoringAppService(
        IServiceOrderRepository serviceOrderRepository,
        IRepository<Service, Guid> serviceRepository)
    {
        this.serviceOrderRepository = serviceOrderRepository;
        this.serviceRepository = serviceRepository;
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.Default)]
    public async Task<ServiceExecutionMonitoringOverviewDto> GetAllServicesMonitoringAsync()
    {
        var monitoringData = await GetServiceMonitoringDataAsync();
        return BuildOverviewDto(monitoringData);
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.Default)]
    public async Task<ServiceExecutionMonitoringDto> GetServiceMonitoringAsync(Guid serviceId)
    {
        var service = await serviceRepository.GetAsync(serviceId);

        var monitoringData = await GetServiceMonitoringDataAsync(serviceId: serviceId);

        var serviceData = monitoringData.FirstOrDefault();
        if (serviceData == null)
        {
            return new ServiceExecutionMonitoringDto
            {
                ServiceId = serviceId,
                ServiceName = service.Description,
                EstimatedTimeInHours = service.EstimatedTime,
                AverageRealExecutionTimeInHours = 0,
                ExecutionCount = 0,
                VarianceInHours = 0,
                VariancePercentage = 0
            };
        }

        return serviceData;
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.Default)]
    public async Task<ServiceExecutionMonitoringOverviewDto> GetCustomerServiceMonitoringAsync(Guid customerId)
    {
        var monitoringData = await GetServiceMonitoringDataAsync(customerId: customerId);
        return BuildOverviewDto(monitoringData);
    }

    [Authorize(GarageManagementPermissions.ServiceOrders.Default)]
    public async Task<ServiceExecutionMonitoringOverviewDto> GetVehicleServiceMonitoringAsync(Guid vehicleId)
    {
        var monitoringData = await GetServiceMonitoringDataAsync(vehicleId: vehicleId);
        return BuildOverviewDto(monitoringData);
    }

    private async Task<List<ServiceExecutionMonitoringDto>> GetServiceMonitoringDataAsync(
        Guid? serviceId = null,
        Guid? customerId = null,
        Guid? vehicleId = null)
    {
        var query = await serviceOrderRepository.GetQueryableAsync();

        var completedServiceOrders = query
            .Where(so => so.Status == ServiceOrderStatus.Finished 
                      || so.Status == ServiceOrderStatus.Delivered 
                      || so.Status == ServiceOrderStatus.Closed)
            .ToList();

        if (customerId.HasValue)
        {
            completedServiceOrders = completedServiceOrders
                .Where(so => so.CustomerId == customerId.Value)
                .ToList();
        }

        if (vehicleId.HasValue)
        {
            completedServiceOrders = completedServiceOrders
                .Where(so => so.VehicleId == vehicleId.Value)
                .ToList();
        }

        var serviceOrdersWithDetails = new List<ServiceOrder>();
        foreach (var so in completedServiceOrders)
        {
            var soWithDetails = await serviceOrderRepository
                .GetWithDetailsAsync(so.Id);
                
            serviceOrdersWithDetails.Add(soWithDetails);
        }

        var serviceExecutionTimes = new Dictionary<Guid, List<ServiceExecutionRecord>>();

        foreach (var serviceOrder in serviceOrdersWithDetails)
        {
            if (!serviceOrder.ExecutionStartedAt.HasValue || !serviceOrder.FinishedAt.HasValue)
                continue;

            var executionTime = (serviceOrder.FinishedAt.Value - serviceOrder.ExecutionStartedAt.Value).TotalHours;

            if (serviceOrder.EstimateId.HasValue)
            {
                var estimate = serviceOrder.Estimate;
                if (estimate?.ServiceItems != null)
                {
                    foreach (var serviceItem in estimate.ServiceItems)
                    {
                        if (serviceId.HasValue && serviceItem.ServiceId != serviceId.Value)
                            continue;

                        if (!serviceExecutionTimes.ContainsKey(serviceItem.ServiceId))
                        {
                            serviceExecutionTimes[serviceItem.ServiceId] = new List<ServiceExecutionRecord>();
                        }

                        serviceExecutionTimes[serviceItem.ServiceId].Add(new ServiceExecutionRecord
                        {
                            ExecutionTimeInHours = executionTime,
                            ServiceItem = serviceItem
                        });
                    }
                }
            }
        }

        var result = new List<ServiceExecutionMonitoringDto>();

        foreach (var serviceId_kvp in serviceExecutionTimes)
        {
            var serviceGuid = serviceId_kvp.Key;
            var executionRecords = serviceId_kvp.Value;

            var service = await serviceRepository.GetAsync(serviceGuid);

            var averageExecutionTime = executionRecords.Average(r => r.ExecutionTimeInHours);
            var minExecutionTime = executionRecords.Min(r => r.ExecutionTimeInHours);
            var maxExecutionTime = executionRecords.Max(r => r.ExecutionTimeInHours);

            var variance = averageExecutionTime - service.EstimatedTime;
            var variancePercentage = service.EstimatedTime > 0 
                ? (variance / service.EstimatedTime) * 100 
                : 0;

            result.Add(new ServiceExecutionMonitoringDto
            {
                ServiceId = serviceGuid,
                ServiceName = service.Description,
                EstimatedTimeInHours = service.EstimatedTime,
                AverageRealExecutionTimeInHours = averageExecutionTime,
                MinExecutionTimeInHours = minExecutionTime,
                MaxExecutionTimeInHours = maxExecutionTime,
                ExecutionCount = executionRecords.Count,
                VarianceInHours = variance,
                VariancePercentage = variancePercentage
            });
        }

        return result.OrderByDescending(m => m.ExecutionCount).ToList();
    }

    private ServiceExecutionMonitoringOverviewDto BuildOverviewDto(List<ServiceExecutionMonitoringDto> servicesData)
    {
        var overview = new ServiceExecutionMonitoringOverviewDto
        {
            Services = servicesData,
            TotalServiceOrders = servicesData.Sum(s => s.ExecutionCount)
        };

        if (servicesData.Count > 0)
        {
            overview.OverallAverageExecutionTimeInHours = servicesData
                .Average(s => s.AverageRealExecutionTimeInHours);

            overview.OverallAverageEstimatedTimeInHours = servicesData
                .Average(s => s.EstimatedTimeInHours);

            overview.AverageAccuracyPercentage = servicesData
                .Average(s => 100 - Math.Abs(s.VariancePercentage));
        }

        return overview;
    }

    private class ServiceExecutionRecord
    {
        public double ExecutionTimeInHours { get; set; }
        public required EstimateServiceItem ServiceItem { get; set; }
    }
}
