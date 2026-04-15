using AutoMapper;
using GarageManagement.Customers;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.Services;
using GarageManagement.Vehicles;

namespace GarageManagement;

public class GarageManagementApplicationMappers : Profile
{
    public GarageManagementApplicationMappers()
    {
        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Document, opt => opt.MapFrom(src => src.Document.Value));
        CreateMap<CustomerCreateUpdateDto, Customer>()
            .ForMember(dest => dest.Document, opt => opt.MapFrom(src => new Document(src.Document)))
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Inventory mappings
        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price));
        CreateMap<InventoryCreateUpdateDto, Inventory>()
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<ProductCreateUpdateDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Service mappings
        CreateMap<Service, ServiceDto>();
        CreateMap<ServiceCreateUpdateDto, Service>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Vehicle mappings
        CreateMap<Vehicle, VehicleDto>();
        CreateMap<VehicleCreateUpdateDto, Vehicle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
