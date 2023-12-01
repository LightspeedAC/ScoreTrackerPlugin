using AssettoServer.Network.Tcp;
using ScoreTrackerPlugin.Session;

namespace ScoreTrackerPlugin.Managers;

public class ScoreTrackerSessionManager
{

    private readonly ScoreTrackerPlugin _plugin;
    private Dictionary<ulong, ScoreTrackerSession> _sessions = new();

    public ScoreTrackerSessionManager(ScoreTrackerPlugin plugin)
    {
        _plugin = plugin;
    }

    public Dictionary<ulong, ScoreTrackerSession> GetSessions()
    {
        return _sessions;
    }

    public ScoreTrackerSession? GetSession(ACTcpClient client)
    {
        if (_sessions.ContainsKey(client.Guid))
        {
            return _sessions[client.Guid];
        }

        return null;
    }

    public ScoreTrackerSession? AddSession(ACTcpClient client)
    {
        if (GetSession(client) != null)
        {
            return null;
        }

        ScoreTrackerSession newSession = new(client, _plugin);
        _sessions[client.Guid] = newSession;

        return newSession;
    }

    public void RemoveSession(ACTcpClient client)
    {
        ScoreTrackerSession? session = GetSession(client);
        if (session == null)
        {
            return;
        }

        session.OnRemove();
        _sessions.Remove(client.Guid);
    }
}
