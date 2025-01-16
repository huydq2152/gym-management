using GymManagement.EventBus.Messages.IntegrationEvents.Events;
using Newtonsoft.Json;

namespace GymManagement.EventProcessor.RoomCosmosDbChangeFeed;

public class RoomCosmosDbChangeFeedDocument
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("data")]
    public RoomCosmosDbChangeFeedEvent Data { get; set; }

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; }
}