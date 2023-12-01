using AssettoServer.Server.Configuration;

namespace ScoreTrackerPlugin;

public class ScoreTrackerConfiguration : IValidateConfiguration<ScoreTrackerConfigurationValidator>
{
    
    public int ServerType { get; init; } = 1;//0 = overtake scores, 1 = drift scores, 2 = timed laps
    public bool BroadcastMessages { get; init; } = true;//whether the server sends a message in chat for each new personal best overtake score, drift score, or lap time.
}
