using System;
using GarageManagement.Permissions;
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
            GetPolicyName = GarageManagementPermissions.Services.Default;
            GetListPolicyName = GarageManagementPermissions.Services.Default;
            CreatePolicyName = GarageManagementPermissions.Services.Create;
            UpdatePolicyName = GarageManagementPermissions.Services.Edit;
            DeletePolicyName = GarageManagementPermissions.Services.Delete;
        }
    }
}