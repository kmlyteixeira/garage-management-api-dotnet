using System;
using System.Collections.Generic;

namespace GarageManagement.ServiceOrders;

/// <summary>
/// Represents monitoring data for a service execution time.
/// </summary>
public class ServiceExecutionMonitoringDto
{
    /// <summary>
    /// Service ID.
    /// </summary>
    public Guid ServiceId { get; set; }

    /// <summary>
    /// Service name.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time to complete the service in hours.
    /// </summary>
    public double EstimatedTimeInHours { get; set; }

    /// <summary>
    /// Average actual execution time in hours (based on completed service orders).
    /// </summary>
    public double AverageRealExecutionTimeInHours { get; set; }

    /// <summary>
    /// Minimum execution time in hours.
    /// </summary>
    public double? MinExecutionTimeInHours { get; set; }

    /// <summary>
    /// Maximum execution time in hours.
    /// </summary>
    public double? MaxExecutionTimeInHours { get; set; }

    /// <summary>
    /// Total number of service orders with this service that were executed.
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Difference between estimated and average real time (positive = underestimated, negative = overestimated).
    /// </summary>
    public double VarianceInHours { get; set; }

    /// <summary>
    /// Variance percentage: ((Real - Estimated) / Estimated) * 100.
    /// </summary>
    public double VariancePercentage { get; set; }
}

/// <summary>
/// Represents overall monitoring data for service executions.
/// </summary>
public class ServiceExecutionMonitoringOverviewDto
{
    /// <summary>
    /// List of services with their execution metrics.
    /// </summary>
    public List<ServiceExecutionMonitoringDto> Services { get; set; } = new();

    /// <summary>
    /// Total number of service orders monitored.
    /// </summary>
    public int TotalServiceOrders { get; set; }

    /// <summary>
    /// Average estimation accuracy across all services (as a percentage).
    /// </summary>
    public double AverageAccuracyPercentage { get; set; }

    /// <summary>
    /// Overall average execution time in hours.
    /// </summary>
    public double OverallAverageExecutionTimeInHours { get; set; }

    /// <summary>
    /// Overall average estimated time in hours.
    /// </summary>
    public double OverallAverageEstimatedTimeInHours { get; set; }
}
