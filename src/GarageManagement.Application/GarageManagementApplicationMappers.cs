using AutoMapper;
using GarageManagement.Customers;
using GarageManagement.Estimates;
using GarageManagement.Inventories;
using GarageManagement.Products;
using GarageManagement.Services;
using GarageManagement.ServiceOrders;
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
        CreateMap<Inventory, InventoryDto>(); ;
        CreateMap<InventoryCreateUpdateDto, Inventory>()
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Inventory, InventoryGetListInputDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.MaxQuantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.MinQuantity, opt => opt.MapFrom(src => src.Quantity));

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

        // Estimate mappings
        CreateMap<EstimateServiceItem, EstimateServiceItemDto>();
        CreateMap<EstimateServiceItemCreateDto, EstimateServiceItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EstimateId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
        CreateMap<EstimateProductItem, EstimateProductItemDto>();
        CreateMap<EstimateProductItemCreateDto, EstimateProductItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EstimateId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
        CreateMap<Estimate, EstimateDto>();
        CreateMap<EstimateCreateUpdateDto, Estimate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.SentAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
            .ForMember(dest => dest.ConvertedToServiceOrderId, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceItems, opt => opt.Ignore())
            .ForMember(dest => dest.PartItems, opt => opt.Ignore());

        // Service order mappings
        CreateMap<ServiceOrder, ServiceOrderDto>();
        CreateMap<ServiceOrderCreateDto, ServiceOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DiagnosisStartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ExecutionStartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FinishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveredAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.Estimate, opt => opt.Ignore());
        CreateMap<ServiceOrderUpdateDto, ServiceOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceOrderNumber, opt => opt.Ignore())
            .ForMember(dest => dest.EstimateId, opt => opt.Ignore())
            .ForMember(dest => dest.Estimate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DiagnosisStartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ExecutionStartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.FinishedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveredAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.VehicleId, opt => opt.Ignore());
    }
}
