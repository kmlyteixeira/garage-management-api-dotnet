using System;
using Volo.Abp.Application.Services;

namespace GarageManagement.Products;

public interface IProductAppService : ICrudAppService<ProductDto, Guid, ProductGetListInputDto, ProductCreateUpdateDto>
{
}
