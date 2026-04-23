using System;
using GarageManagement.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Products
{
    public class ProductAppService :
        CrudAppService<Product, ProductDto, Guid, ProductGetListInputDto, ProductCreateUpdateDto>,
        IProductAppService
    {
        public ProductAppService(IRepository<Product, Guid> repository) : base(repository)
        {
            GetPolicyName = GarageManagementPermissions.Products.Default;
            GetListPolicyName = GarageManagementPermissions.Products.Default;
            CreatePolicyName = GarageManagementPermissions.Products.Create;
            UpdatePolicyName = GarageManagementPermissions.Products.Edit;
            DeletePolicyName = GarageManagementPermissions.Products.Delete;
        }
    }
}