using System;
using System.Threading.Tasks;
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
        }

        public override async Task<CustomerDto> CreateAsync(CustomerCreateUpdateDto input)
        {
            try
            {
                return await base.CreateAsync(input);
            }
            catch (Exception ex) when (ContainsConstraintName(ex, "IX_AppCustomers_Document"))
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
            catch (Exception ex) when (ContainsConstraintName(ex, "IX_AppCustomers_Document"))
            {
                throw new UserFriendlyException("Ja existe um cliente cadastrado com este documento.");
            }
        }

        private static bool ContainsConstraintName(Exception ex, string constraintName)
        {
            if (ex.Message.Contains(constraintName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ex.InnerException != null && ContainsConstraintName(ex.InnerException, constraintName);
        }
    }
}