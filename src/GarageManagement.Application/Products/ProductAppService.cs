using System;
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
        }
    }
}