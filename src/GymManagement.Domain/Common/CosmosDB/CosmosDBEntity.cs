using Newtonsoft.Json;

namespace GymManagement.Domain.Common.CosmosDB;

public class CosmosDBEntity
{
    [JsonProperty(PropertyName = "id")]
    public virtual Guid Id { get; set; }
}