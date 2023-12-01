using AssettoServer.Server;
using AssettoServer.Network.Tcp;
using Serilog;
using ScoreTrackerPlugin.Managers;
using AssettoServer.Shared.Network.Packets;
using ScoreTrackerPlugin.Session;
using AssettoServer.Server.Configuration;
using System.Text;
using AssettoServer.Shared.Network.Packets.Shared;

namespace ScoreTrackerPlugin;

public class ScoreTrackerPlugin
{

    private const string LOG_PREFIX = "[Score-Tracker] ";
    public const string NO_NAME = "Unknown";

    public const string ROOT_DIR = "score-tracker";
    public const string PLAYER_SCORES_DIR = $"{ROOT_DIR}/scores";//each client will have their own folder containing a file with their score/stats
    public const string PLAYER_LAP_TIMES_DIR = $"{ROOT_DIR}/laptimes";

    public const string PLAYER_SCORES_FILE = "scores.json";//every client has their own, located in PLAYER_SCORES_DIR/(steamid)/
    public const string PLAYER_LAP_TIMES_FILE = "laptimes.json";

    private readonly ACServerConfiguration _acServerConfiguration;
    private readonly EntryCarManager _entryCarManager;
    public readonly ScoreTrackerSessionManager _scoreTrackerSessionManager;
    private readonly ScoreTrackerUtils _scoreTrackerUtils;
    public readonly SessionManager _sessionManager;
    private readonly CSPClientMessageTypeManager _cspClientMessageTypeManager;
    private readonly ScoreTrackerConfiguration _scoreTrackerConfiguration;

    private bool _registerLapTimings = false;

    public ScoreTrackerPlugin(ACServerConfiguration acServerConfiguration, EntryCarManager entryCarManager, SessionManager sessionManager, CSPClientMessageTypeManager cspClientMessageTypeManager, ScoreTrackerConfiguration scoreTrackerConfiguration)
    {
        Log.Information("------------------------------------");
        Log.Information("ScoreTrackerPlugin");
        Log.Information("By Jonfinity");
        Log.Information("------------------------------------");

        _acServerConfiguration = acServerConfiguration;
        _entryCarManager = entryCarManager;
        _scoreTrackerSessionManager = new ScoreTrackerSessionManager(this);
        _scoreTrackerUtils = new ScoreTrackerUtils(this);
        _sessionManager = sessionManager;
        _cspClientMessageTypeManager = cspClientMessageTypeManager;
        _scoreTrackerConfiguration = scoreTrackerConfiguration;
        _entryCarManager.ClientConnected += OnClientConnected;
        _entryCarManager.ClientDisconnected += OnClientDisconnected;

        if (!Directory.Exists(ROOT_DIR))
        {
            Directory.CreateDirectory(ROOT_DIR);
        }
        if (!Directory.Exists(PLAYER_SCORES_DIR))
        {
            Directory.CreateDirectory(PLAYER_SCORES_DIR);
        }
        if (!Directory.Exists(PLAYER_LAP_TIMES_DIR))
        {
            Directory.CreateDirectory(PLAYER_LAP_TIMES_DIR);
        }

        System.Timers.Timer timer = new(1000);
        timer.Elapsed += new System.Timers.ElapsedEventHandler(_scoreTrackerUtils.UpdateClientSessions);
        timer.Start();

        RegisterScoreType();
    }

    private void RegisterScoreType()
    {
        if (IsTrackingCutUpScores())
        {
            _cspClientMessageTypeManager.RegisterClientMessageType(0x953A6FB5, new Action<ACTcpClient, PacketReader>(IncomingScoreEnd));
            Log.Information($"{LOG_PREFIX}Now tracking 'cut-up scores'!");
        }
        else if (IsTrackingDriftScores())
        {
            _cspClientMessageTypeManager.RegisterClientMessageType(0x9D532A4E, new Action<ACTcpClient, PacketReader>(IncomingScoreEnd));
            Log.Information($"{LOG_PREFIX}Now tracking 'drift scores'!");
        }
        else if (IsTrackingLapTimes())
        {
            _registerLapTimings = true;
            Log.Information($"{LOG_PREFIX}Now tracking 'lap-times'!");
        }
    }

    private void OnClientConnected(ACTcpClient client, EventArgs args)
    {
        if (_registerLapTimings)
        {
            client.LapCompleted += OnClientLapCompleted;
        }
        _scoreTrackerSessionManager.AddSession(client);
    }

    private void OnClientDisconnected(ACTcpClient client, EventArgs args)
    {
        client.LapCompleted -= OnClientLapCompleted;
        _scoreTrackerSessionManager.RemoveSession(client);
    }

    private void OnClientLapCompleted(ACTcpClient client, LapCompletedEventArgs args)
    {
        if (args.Packet.Cuts > 0)
        {
            return;
        }

        uint lapTimeMs = args.Packet.LapTime;
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(lapTimeMs);
        string lapTime = timeSpan.ToString(@"mm\:ss\:fff");
        ScoreTrackerSession? session = _scoreTrackerSessionManager.GetSession(client);
        if (session == null || (session._bestLapTime != 0 && lapTimeMs >= session._bestLapTime))
        {
            return;
        }

        Log.Information($"{LOG_PREFIX}Lap-time of {lapTime} from {session._username}");
        if (_scoreTrackerConfiguration.BroadcastMessages)
        {
            _entryCarManager.BroadcastPacket(
            new ChatMessage
            {
                SessionId = 255,
                Message = $"{session._username} just completed a new personal best lap-time of {lapTime}!"
            }
        );
        }
        session._bestLapTime = lapTimeMs;
        session.SaveClientLapTimesData(session._bestLapTime, session._car, session._track);
    }

    private void IncomingScoreEnd(ACTcpClient client, PacketReader reader)
    {
        long score = reader.Read<long>();
        int multiplier = reader.Read<int>();
        string car = reader.ReadStringFixed(Encoding.UTF8, 64);
        //string track = reader.ReadStringFixed(Encoding.UTF8, 64);//TODO
        ScoreTrackerSession? session = _scoreTrackerSessionManager.GetSession(client);
        if (session == null || score <= session._topScore)
        {
            return;
        }
        Log.Information($"{LOG_PREFIX}Score of {score} ({multiplier}x) from {session._username}, in the '{car}'");
        if (_scoreTrackerConfiguration.BroadcastMessages)
        {
            _entryCarManager.BroadcastPacket(
                        new ChatMessage
                        {
                            SessionId = 255,
                            Message = $"{session._username} just scored a new personal best score of {score} ({multiplier}x)!"
                        }
                    );
        }

        session._topScore = score;
        session._multiplier = multiplier;
        session._averageSpeedAtTopScore = session.GetAverageSpeed();
        session._car = car;
        session._track = _acServerConfiguration.Server.Track;
        session.SaveClientScoreData(session._topScore, session._multiplier, session._averageSpeedAtTopScore, session._car, session._track);
    }

    public bool IsTrackingCutUpScores()
    {
        return _scoreTrackerConfiguration.ServerType == 0;
    }

    public bool IsTrackingDriftScores()
    {
        return _scoreTrackerConfiguration.ServerType == 1;
    }

    public bool IsTrackingLapTimes()
    {
        return _scoreTrackerConfiguration.ServerType == 2;
    }
}
