using SpeedrunDotComAPI.Levels;
using SpeedrunDotComAPI.Runs;
using SpeedrunDotComAPI.Users;
using WinstonBot.Exceptions;
using WinstonBot.Models;

namespace WinstonBot.Extensions;

public static class RunModelExtensions
{
    public static bool HasVariable(this RunModel run, string variableId, string expectedValueId)
    {
        return run.Values.TryGetValue(variableId, out var currentValueId) && currentValueId == expectedValueId;
    }

    public static Run ToRun(this RunModel run)
    {
        if (run.Category is not string category)
            throw new ArgumentException("Required submission object Category missing", nameof(run));

        var verified = GetVerificationStatus(run);
        var players = GetPlayers(run);
        var video = GetVideo(run);
        var variables = GetVariables(run);
        var times = new Times { InGame = run.Times.IngameTime, RealTime = run.Times.RealTimeTime, RealTimeNoLoads = run.Times.RealtimeNoloadsTime };

        return new Run
        {
            Category = category,
            Level = run.Level is string level ? level : null,
            Date = run.Date,
            Region = run.System.Region,
            Platform = run.System.Platform,
            Verified = verified,
            Times = times,
            Players = players,
            Emulated = run.System.Emulated,
            Video = video,
            Comment = run.Comment,
            Splitsio = run.Splits?.Uri.ToString(),
            Variables = variables,
        };
    }

    private static bool? GetVerificationStatus(RunModel run) => run.Status.Status switch
    {
        RunStatus.New => null,
        RunStatus.Verified => true,
        RunStatus.Rejected => false,
        _ => throw new ArgumentOutOfRangeException(nameof(run), "Unknown run Status"),
    };

    private static Player[] GetPlayers(RunModel run) => run.Players switch
    {
        UserListModel userListModel => userListModel
            .Data.Select(user => new Player { Id = user.Id, Rel = (Models.UserRole)user.Role })
            .ToArray(),
        RunPlayerModel[] runPlayerModels => runPlayerModels
            .Select(x => new Player { Id = x.Id, Rel = (Models.UserRole)x.Rel })
            .ToArray(),
        _ => throw new NullPlayerException() // We don't allow submissions without players, so this should never happen.
    };

    private static string GetVideo(RunModel run)
    {
        if (run.Videos is null)
        {
            throw new NullVideoException(); // We don't allow submissions without a video, so this should never happen.
        }

        if (run.Videos.Value.Links.Length == 1)
        {
            return run.Videos.Value.Links[0].Uri.ToString();
        }

        /* Multiple links show when one or more videos are provided in the comment (meaning there must be a comment).
        /  There's no definitive way to discern which one is the displayed video on SRC.
        /  Try to find one that isn't in the comment, otherwise use the first of the links returned (if all are in comments).
        */
        var first = run.Videos.Value.Links[0];
        var chosen = run.Videos.Value.Links.FirstOrDefault(v => !run.Comment!.Contains(v.Uri.ToString()), first);
        return chosen.Uri.ToString();
    }

    // TODO: No way to discern between pre-defined and user-defined here, assuming pre-defined.
    private static Dictionary<string, Variable> GetVariables(RunModel run) =>
        run.Values
           .Select(entry => new KeyValuePair<string, Variable>(entry.Key, new Variable { Type = VariableType.Predefined, Value = entry.Value }))
           .ToDictionary();
}