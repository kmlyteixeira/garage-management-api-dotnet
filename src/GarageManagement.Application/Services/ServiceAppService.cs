using System;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Services
{
    public class ServiceAppService :
        CrudAppService<Service, ServiceDto, Guid, ServiceGetListInputDto, ServiceCreateUpdateDto>,
        IServiceAppService
    {
        public ServiceAppService(IRepository<Service, Guid> repository) : base(repository)
        {
        }
    }
}