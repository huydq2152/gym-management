using GymManagement.Domain.Admins.CosmosDB;

namespace GymManagement.Application.Common.Interfaces.CosmosDB;

public interface ICosmosDBAdminRepository: ICosmosDBRepository<CosmosDBAdmin>
{
    
}