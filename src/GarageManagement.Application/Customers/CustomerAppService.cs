using System;
using System.Threading.Tasks;
using GarageManagement.Permissions;
using GarageManagement.Shared;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Customers
{
    public class CustomerAppService :
        CrudAppService<Customer, CustomerDto, Guid, CustomerGetListInputDto, CustomerCreateUpdateDto>,
        ICustomerAppService
    {
        public CustomerAppService(IRepository<Customer, Guid> repository) : base(repository)
        {
            GetPolicyName = GarageManagementPermissions.Customers.Default;
            GetListPolicyName = GarageManagementPermissions.Customers.Default;
            CreatePolicyName = GarageManagementPermissions.Customers.Create;
            UpdatePolicyName = GarageManagementPermissions.Customers.Edit;
            DeletePolicyName = GarageManagementPermissions.Customers.Delete;
        }

        public override async Task<CustomerDto> CreateAsync(CustomerCreateUpdateDto input)
        {
            try
            {
                return await base.CreateAsync(input);
            }
            catch (Exception ex) when (DbConstraintExceptionHelper.ContainsConstraintName(ex, "IX_AppCustomers_Document"))
            {
                throw new UserFriendlyException("Ja existe um cliente cadastrado com este documento.");
            }
        }

        public override async Task<CustomerDto> UpdateAsync(Guid id, CustomerCreateUpdateDto input)
        {
            try
            {
                return await base.UpdateAsync(id, input);
            }
            catch (Exception ex) when (DbConstraintExceptionHelper.ContainsConstraintName(ex, "IX_AppCustomers_Document"))
            {
                throw new UserFriendlyException("Ja existe um cliente cadastrado com este documento.");
            }
        }

        [Authorize(GarageManagementPermissions.Customers.Edit)]
        public async Task<CustomerDto> SetActiveAsync(Guid id, CustomerSetActiveDto input)
        {
            var customer = await Repository.GetAsync(id);
            customer.SetActive(input.IsActive);
            await Repository.UpdateAsync(customer, autoSave: true);
            return ObjectMapper.Map<Customer, CustomerDto>(customer);
        }
    }
}