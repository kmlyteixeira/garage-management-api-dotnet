using System;
using Volo.Abp.Application.Services;

namespace GarageManagement.Customers;

public interface ICustomerAppService : ICrudAppService<CustomerDto, Guid, CustomerGetListInputDto, CustomerCreateUpdateDto>
{
}
