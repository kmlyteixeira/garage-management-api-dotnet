using System.Threading.Tasks;

namespace GarageManagement.Data;

public interface IGarageManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
