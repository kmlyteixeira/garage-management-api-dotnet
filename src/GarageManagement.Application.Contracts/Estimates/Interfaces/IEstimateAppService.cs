using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GarageManagement.Estimates;

public interface IEstimateAppService : IReadOnlyAppService<EstimateDto, Guid, EstimateGetListInputDto>
{
	Task<EstimateDto> ApproveAsync(Guid id);

	Task<EstimateDto> RejectAsync(Guid id, EstimateRejectDto input);
}
