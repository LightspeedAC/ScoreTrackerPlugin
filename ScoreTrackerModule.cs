using AssettoServer.Server.Plugin;
using Autofac;

namespace ScoreTrackerPlugin;

public class ScoreTrackerModule : AssettoServerModule<ScoreTrackerConfiguration>
{
    
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ScoreTrackerPlugin>().AsSelf().AutoActivate().SingleInstance();
    }
}
