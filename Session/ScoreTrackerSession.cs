using AssettoServer.Network.Tcp;
using System.Text.Json;

namespace ScoreTrackerPlugin.Session;

public sealed class ScoreTrackerSession
{

    private readonly ScoreTrackerPlugin _plugin;
    public string _username = ScoreTrackerPlugin.NO_NAME;

    public ACTcpClient _client;
    public long _creationTime = 0;
    private readonly Queue<int> _speedList = new();
    public string _car = "";
    public string _track = "";

    public uint _bestLapTime = 0;

    private int _topSpeed = 0;

    public long _topScore = 0;
    public long _multiplier = 0;
    public long _averageSpeedAtTopScore = 0;

    public class ClientScoresData
    {
        public string Username { get; set; } = ScoreTrackerPlugin.NO_NAME;
        public ulong GUID { get; set; } = 0;
        public long TopScore { get; set; } = 0;
        public long Multiplier { get; set; } = 0;
        public long AverageSpeedAtTopScore { get; set; } = 0;
        public string CarAtTopScore { get; set; } = "";
        public string TrackAtTopScore { get; set; } = "";
    }

    public class ClientLapTimesData
    {
        public string Username { get; set; } = ScoreTrackerPlugin.NO_NAME;
        public ulong GUID { get; set; } = 0;
        public uint BestLapTime { get; set; } = 0;
        public string CarAtBestLapTime { get; set; } = "";
        public string TrackAtBestLapTime { get; set; } = "";
    }

    public ScoreTrackerSession(ACTcpClient client, ScoreTrackerPlugin plugin)
    {
        string name = client.Name ?? ScoreTrackerPlugin.NO_NAME;

        _client = client;
        _creationTime = plugin._sessionManager.ServerTimeMilliseconds;

        _plugin = plugin;
        _username = ScoreTrackerUtils.SanitizeUsername(name);

        LoadClientScoresData();
        LoadClientLaptimesData();
    }

    public void OnRemove()
    {
        if (!_client.HasSentFirstUpdate || _client.ChecksumStatus != ChecksumStatus.Succeeded)
        {
            return;
        }

        SaveClientScoreData(_topScore, _multiplier, _averageSpeedAtTopScore, _car, _track);
        SaveClientLapTimesData(_bestLapTime, _car, _track);
    }

    private async void LoadClientScoresData()
    {
        string file = $"{ScoreTrackerPlugin.PLAYER_SCORES_DIR}/{_client.Guid}/{ScoreTrackerPlugin.PLAYER_SCORES_FILE}";
        if (File.Exists(file))
        {
            FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > 0)
            {
                ClientScoresData? existingData = (ClientScoresData?)JsonSerializer.Deserialize(stream, typeof(ClientScoresData));
                if (existingData != null)
                {
                    _topScore = existingData.TopScore;
                }
            }
            await stream.DisposeAsync();
            stream.Close();
        }
    }

    private async void CreateClientScoresData()
    {
        string dir = $"{ScoreTrackerPlugin.PLAYER_SCORES_DIR}/{_client.Guid}";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string file = $"{dir}/{ScoreTrackerPlugin.PLAYER_SCORES_FILE}";
        if (!File.Exists(file))
        {
            FileStream f = File.Create(file);
            await f.DisposeAsync();
            f.Close();
        }
    }

    private async void LoadClientLaptimesData()
    {
        string file = $"{ScoreTrackerPlugin.PLAYER_LAP_TIMES_DIR}/{_client.Guid}/{ScoreTrackerPlugin.PLAYER_LAP_TIMES_FILE}";
        if (File.Exists(file))
        {
            FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length > 0)
            {
                ClientLapTimesData? existingData = (ClientLapTimesData?)JsonSerializer.Deserialize(stream, typeof(ClientLapTimesData));
                if (existingData != null)
                {
                    _bestLapTime = existingData.BestLapTime;
                }
            }
            await stream.DisposeAsync();
            stream.Close();
        }
    }

    private async void CreateClientLapTimesData()
    {
        string dir = $"{ScoreTrackerPlugin.PLAYER_LAP_TIMES_DIR}/{_client.Guid}";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string file = $"{dir}/{ScoreTrackerPlugin.PLAYER_LAP_TIMES_FILE}";
        if (!File.Exists(file))
        {
            FileStream f = File.Create(file);
            await f.DisposeAsync();
            f.Close();
        }
    }

    public async void SaveClientScoreData(long score, long multiplier, long avgSpeed, string car, string track)
    {
        string file = $"{ScoreTrackerPlugin.PLAYER_SCORES_DIR}/{_client.Guid}/{ScoreTrackerPlugin.PLAYER_SCORES_FILE}";
        if (!File.Exists(file))
        {
            CreateClientScoresData();
        }

        FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        if (stream.Length > 0)
        {
            ClientScoresData? existingData = (ClientScoresData?)JsonSerializer.Deserialize(stream, typeof(ClientScoresData));
            if (existingData == null || existingData.TopScore >= score)
            {
                await stream.DisposeAsync();
                stream.Close();
                return;
            }

            stream.SetLength(0);
        }

        ClientScoresData data = new() { Username = _username, GUID = _client.Guid, TopScore = score, Multiplier = multiplier, AverageSpeedAtTopScore = avgSpeed, CarAtTopScore = car, TrackAtTopScore = track };
        await JsonSerializer.SerializeAsync(stream, data);
        await stream.DisposeAsync();
        stream.Close();
    }

    public async void SaveClientLapTimesData(uint lapTime, string car, string track)
    {
        string file = $"{ScoreTrackerPlugin.PLAYER_LAP_TIMES_DIR}/{_client.Guid}/{ScoreTrackerPlugin.PLAYER_LAP_TIMES_FILE}";
        if (!File.Exists(file))
        {
            CreateClientLapTimesData();
        }

        FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        if (stream.Length > 0)
        {
            ClientLapTimesData? existingData = (ClientLapTimesData?)JsonSerializer.Deserialize(stream, typeof(ClientLapTimesData));
            if (existingData == null || (existingData.BestLapTime != 0 && existingData.BestLapTime <= lapTime))
            {
                await stream.DisposeAsync();
                stream.Close();
                return;
            }

            stream.SetLength(0);
        }

        ClientLapTimesData data = new() { Username = _username, GUID = _client.Guid, BestLapTime = lapTime, CarAtBestLapTime = car, TrackAtBestLapTime = track };
        await JsonSerializer.SerializeAsync(stream, data);
        await stream.DisposeAsync();
        stream.Close();
    }

    public string GetUsername()
    {
        return _username;
    }

    public int GetAverageSpeed()
    {
        if (_speedList.Count < 1)
        {
            return 0;
        }

        return _speedList.Sum() / _speedList.Count;
    }

    public Queue<int> GetSpeedList()
    {
        return _speedList;
    }

    public void AddToAverageSpeed(int speed)
    {
        if (_speedList.Count > 25)
        {
            _speedList.Dequeue();
        }

        _speedList.Enqueue(speed);
    }

    public double CalculateDistanceDriven()
    {
        double hours = TimeSpan.FromMilliseconds(CalculateTimeSpent()).TotalHours;
        long avgSpeed = GetAverageSpeed();
        return avgSpeed * hours;
    }

    public int GetTopSpeed()
    {
        return _topSpeed;
    }

    public void SetTopSpeed(int speed)
    {
        _topSpeed = speed;
    }

    public long CalculateTimeSpent()
    {
        return _plugin._sessionManager.ServerTimeMilliseconds - _creationTime;
    }
}
