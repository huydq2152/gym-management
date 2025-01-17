namespace GymManagement.EventProcessor.Configurations;

public class EventBusSettings
{
    public string HostAddress { get; set; }
    public Topics Topics { get; set; }
}

public class Topics
{
    public TopicSetting RoomCosmosDbChangeFeed { get; set; }
}

public class TopicSetting
{
    public string Name { get; set; }
}