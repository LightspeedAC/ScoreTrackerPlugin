using AssettoServer.Server.Configuration;

namespace ScoreTrackerPlugin;

public class ScoreTrackerConfiguration : IValidateConfiguration<ScoreTrackerConfigurationValidator>
{
    
    public int ServerType { get; init; } = 1;//0 = cut-up, 1 = drift, 2 = timed laps
    public bool BroadcastMessages { get; init; } = true;//whether to broadcast new pb's in chat
}
