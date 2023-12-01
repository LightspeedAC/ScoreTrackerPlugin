using AssettoServer.Network.Tcp;
using AssettoServer.Server;
using System.Text.RegularExpressions;
using ScoreTrackerPlugin.Session;

namespace ScoreTrackerPlugin;

public class ScoreTrackerUtils
{

    private readonly ScoreTrackerPlugin _plugin;

    private static readonly string[] _forbiddenUsernameSubstrings = { "discord", "@", "#", ":", "```" };
    private static readonly string[] _forbiddenUsernames = { "everyone", "here" };

    public ScoreTrackerUtils(ScoreTrackerPlugin plugin)
    {
        _plugin = plugin;
    }

    public void UpdateClientSessions(object? sender, EventArgs e)
    {
        foreach (var dictSession in _plugin._scoreTrackerSessionManager.GetSessions())
        {
            ScoreTrackerSession session = dictSession.Value;
            if (session.GetType() == typeof(ScoreTrackerSession))
            {
                ACTcpClient client = session._client;
                if (client.IsConnected && client.HasSentFirstUpdate)
                {
                    EntryCar car = client.EntryCar;
                    if (car != null)
                    {
                        int speedKmh = (int)(car.Status.Velocity.Length() * 3.6f);
                        session.AddToAverageSpeed(speedKmh);
                        if (speedKmh > session.GetTopSpeed())
                        {
                            session.SetTopSpeed(speedKmh);
                        }
                    }
                }
            }
        }
    }

    public static string SanitizeUsername(string name)
    {
        foreach (string str in _forbiddenUsernames)
        {
            if (name == str)
            {
                return $"_{str}";
            }
        }

        foreach (string str in _forbiddenUsernameSubstrings)
        {
            name = Regex.Replace(name, str, new string('*', str.Length), RegexOptions.IgnoreCase);
        }

        name = name[..Math.Min(name.Length, 80)];

        return name;
    }
}