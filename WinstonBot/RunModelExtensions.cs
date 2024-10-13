using SpeedrunDotComAPI.Categories;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Levels;
using SpeedrunDotComAPI.Runs;
using SpeedrunDotComAPI.Users;

namespace WinstonBot;

public static class RunModelExtensions
{
    public static Run ToRun(this RunModel run)
    {
        if (run.Category is null)
            throw new ArgumentException("Required submission object Category missing", nameof(run));
        if (run.Game is null)
            throw new ArgumentException("Required submission object Game missing", nameof(run));

        var categoryModel = (CategoryModel)run.Category;
        var gameModel = (GameModel)run.Game;

        bool? verified = run.Status.Status switch
        {
            RunStatus.New => null,
            RunStatus.Verified => true,
            RunStatus.Rejected => false,
            _ => throw new ArgumentOutOfRangeException(nameof(run), "Unknown run Status"),
        };

        var players = run.Players switch
        {
            UserListModel userListModel => userListModel
                .Data.Select(user => new RunPlayerModel() { Id = user.Id, Rel = user.Role, Uri = user.Weblink, })
                .ToArray(),
            RunPlayerModel[] runPlayerModels => runPlayerModels,
            _ => null,
        };

        string? video;
        if (run.Videos is null)
        {
            video = null;
        }
        else if (run.Videos.Value.Links.Length == 1)
        {
            video = run.Videos.Value.Links[0].Uri.ToString();
        }
        else
        {
            /* Multiple links show when one or more videos are provided in the comment (meaning there must be a comment).
             / There's no definitive way to discern which one is the displayed video on SRC.
             / Try to find one that isn't in the comment, otherwise use the first of the links returned (if all are in comments).
            */
            var first = run.Videos.Value.Links[0];
            var chosen = run.Videos.Value.Links.FirstOrDefault(v => run.Comment!.Contains(v.Uri.ToString()), first);
            video = chosen.Uri.ToString();
        }

        // TODO: No way to discern between pre-defined and user-defined here, assuming pre-defined.
        var variables = run
            .Values.Select(entry =>
                new KeyValuePair<string, Variable>(entry.Key, new Variable { Type = VariableType.Predefined, Value = entry.Value })
            )
            .ToDictionary();

        return new Run
        {
            CategoryModel = categoryModel,
            GameModel = gameModel,
            Category = categoryModel.Id,
            Level = run.Level is LevelModel levelModel ? levelModel.Id : null,
            Date = run.Date,
            Region = run.System.Region,
            Platform = run.System.Platform,
            Verified = verified,
            Times = run.Times,
            Players = players,
            Emulated = run.System.Emulated,
            Video = video,
            Comment = run.Comment,
            Splitsio = run.Splits?.Uri.ToString(),
            Variables = variables,
        };
    }
}