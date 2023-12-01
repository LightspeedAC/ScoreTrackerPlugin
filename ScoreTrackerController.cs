using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ScoreTrackerPlugin.Session;

namespace ScoreTrackerPlugin;

[ApiController]
public class ScoreTrackerController : ControllerBase
{

    private readonly ScoreTrackerPlugin _plugin;

    public ScoreTrackerController(ScoreTrackerPlugin plugin)
    {
        _plugin = plugin;
    }

    [HttpGet("/scores")]
    public string Scores()
    {
        List<ScoreTrackerSession.ClientScoresData> list = new();
        if (_plugin.IsTrackingCutUpScores() || _plugin.IsTrackingDriftScores())
        {
            foreach (string dir in Directory.GetDirectories(ScoreTrackerPlugin.PLAYER_SCORES_DIR))
            {
                string file = $"{dir}/{ScoreTrackerPlugin.PLAYER_SCORES_FILE}";
                if (System.IO.File.Exists(file))
                {

                    FileStream stream = System.IO.File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    if (stream.Length > 0)
                    {
                        ScoreTrackerSession.ClientScoresData? existingData = (ScoreTrackerSession.ClientScoresData?)JsonSerializer.Deserialize(stream, typeof(ScoreTrackerSession.ClientScoresData));
                        if (existingData != null && existingData.GetType() == typeof(ScoreTrackerSession.ClientScoresData))
                        {
                            list.Add(existingData);
                        }

                        stream.Dispose();
                        stream.Close();
                    }
                }
            }
        }

        return JsonSerializer.Serialize(list);
    }

    [HttpGet("/laptimes")]
    public string LapTimes()
    {
        List<ScoreTrackerSession.ClientLapTimesData> list = new();
        if (_plugin.IsTrackingLapTimes())
        {
            foreach (string dir in Directory.GetDirectories(ScoreTrackerPlugin.PLAYER_LAP_TIMES_DIR))
            {
                string file = $"{dir}/{ScoreTrackerPlugin.PLAYER_LAP_TIMES_FILE}";
                if (System.IO.File.Exists(file))
                {

                    FileStream stream = System.IO.File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    if (stream.Length > 0)
                    {
                        ScoreTrackerSession.ClientLapTimesData? existingData = (ScoreTrackerSession.ClientLapTimesData?)JsonSerializer.Deserialize(stream, typeof(ScoreTrackerSession.ClientLapTimesData));
                        if (existingData != null && existingData.GetType() == typeof(ScoreTrackerSession.ClientLapTimesData))
                        {
                            list.Add(existingData);
                        }

                        stream.Dispose();
                        stream.Close();
                    }
                }
            }
        }

        return JsonSerializer.Serialize(list);
    }
}
