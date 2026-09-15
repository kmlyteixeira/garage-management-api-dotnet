using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace GarageManagement.Customers;

public interface ICustomerAppService : ICrudAppService<CustomerDto, Guid, CustomerGetListInputDto, CustomerCreateUpdateDto>
{
	Task<CustomerDto> SetActiveAsync(Guid id, CustomerSetActiveDto input);
}
