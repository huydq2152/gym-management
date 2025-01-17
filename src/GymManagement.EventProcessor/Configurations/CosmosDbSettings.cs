namespace GymManagement.EventProcessor.Configurations;

public class CosmosDbSettings
{
    public string EndpointUrl { get; set; }
    public string PrimaryKey { get; set; }
    public string DatabaseName { get; set; }
    public string ProcessorName { get; set; }
    
    public Containers Containers  { get; set; }
}

public class Containers
{
    public ContainerSetting Audit { get; set; }
    public ContainerSetting Room { get; set; }
    public ContainerSetting RoomLeases { get; set; }
}

public class ContainerSetting
{
    public string Name { get; set; }
    public string PartitionKey { get; set; }
}