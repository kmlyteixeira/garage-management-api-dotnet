using System;
using System.Threading.Tasks;
using GarageManagement.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace GarageManagement.Vehicles
{
    public class VehicleAppService :
        CrudAppService<Vehicle, VehicleDto, Guid, VehicleGetListInputDto, VehicleCreateUpdateDto>,
        IVehicleAppService
    {
        public VehicleAppService(IRepository<Vehicle, Guid> repository) : base(repository)
        {
            GetPolicyName = GarageManagementPermissions.Vehicles.Default;
            GetListPolicyName = GarageManagementPermissions.Vehicles.Default;
            CreatePolicyName = GarageManagementPermissions.Vehicles.Create;
            UpdatePolicyName = GarageManagementPermissions.Vehicles.Edit;
            DeletePolicyName = GarageManagementPermissions.Vehicles.Delete;
        }

        public override async Task<VehicleDto> CreateAsync(VehicleCreateUpdateDto input)
        {
            await EnsureLicensePlateIsUniqueAsync(input.LicensePlate);

            try
            {
                return await base.CreateAsync(input);
            }
            catch (Exception ex) when (ContainsConstraintName(ex, "IX_AppVehicles_LicensePlate"))
            {
                throw new UserFriendlyException("Ja existe um veiculo cadastrado com esta placa.");
            }
        }

        public override async Task<VehicleDto> UpdateAsync(Guid id, VehicleCreateUpdateDto input)
        {
            await EnsureLicensePlateIsUniqueAsync(input.LicensePlate, id);

            try
            {
                return await base.UpdateAsync(id, input);
            }
            catch (Exception ex) when (ContainsConstraintName(ex, "IX_AppVehicles_LicensePlate"))
            {
                throw new UserFriendlyException("Ja existe um veiculo cadastrado com esta placa.");
            }
        }

        private async Task EnsureLicensePlateIsUniqueAsync(string licensePlate, Guid? currentVehicleId = null)
        {
            var normalizedLicensePlate = licensePlate.Trim().ToUpperInvariant();
            var queryable = await Repository.GetQueryableAsync();
            var exists = await AsyncExecuter.AnyAsync(
                queryable,
                vehicle => vehicle.LicensePlate.ToUpper() == normalizedLicensePlate
                    && (!currentVehicleId.HasValue || vehicle.Id != currentVehicleId.Value)
            );

            if (exists)
            {
                throw new UserFriendlyException("Ja existe um veiculo cadastrado com esta placa.");
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