using Microsoft.Extensions.Options;
using SpeedrunDotComAPI.Runs;
using WinstonBot.ApiClient;
using WinstonBot.Configuration;
using WinstonBot.Extensions;
using WinstonBot.Models;

namespace WinstonBot;

public class Worker : BackgroundService
{
    private readonly ImprovedSRCApiClient _client;
    private readonly BotOptions _options;
    private readonly ILogger<Worker> _logger;

    public Worker(ImprovedSRCApiClient client, IOptions<BotOptions> options, ILogger<Worker> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var controlSchemeVariables = new Dictionary<Game, (string VariableId, string TankValueId)>();
        var controlSchemeSubcategories = new Dictionary<Game, (string SubcategoryId, string AllValueId, string TankValueId)>();
        var onlyOnce = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (onlyOnce == 0)
            {
                foreach (var game in _options.Games)
                {
                    var gameVariables = await _client.Games.GetSingleGameVariables(game.Id);

                    var controlSchemeVariable = gameVariables.FirstOrDefault(x => x.Name == "Control Scheme" && !x.IsSubcategory);

                    var tankControlSchemeValue = controlSchemeVariable.Values.Values.FirstOrDefault(x => x.Value.Label == "Tank");

                    controlSchemeVariables.Add(game, (controlSchemeVariable.Id, tankControlSchemeValue.Key));

                    var controlSchemeSubcategory = gameVariables.FirstOrDefault(x => x.Name == "Control Scheme" && x.IsSubcategory);
                    var allControlSchemeSubcategoryValue = controlSchemeSubcategory.Values.Values.FirstOrDefault(x => x.Value.Label == "All");
                    var tankOnlySchemeSubcategoryValue = controlSchemeSubcategory.Values.Values.FirstOrDefault(x => x.Value.Label == "Tank-Only");

                    controlSchemeSubcategories.Add(game, (controlSchemeSubcategory.Id, allControlSchemeSubcategoryValue.Key, tankOnlySchemeSubcategoryValue.Key));
                }

                onlyOnce++;
            }

            foreach (var game in _options.Games)
            {
                var controlSchemeVariable = controlSchemeVariables[game];
                var controlSchemeSubcategory = controlSchemeSubcategories[game];
                var verifiedRuns = await FetchRuns(game);
                var tankRuns = verifiedRuns.Where(x => x.HasVariable(controlSchemeVariable.VariableId, controlSchemeVariable.TankValueId) &&
                    x.HasVariable(controlSchemeSubcategory.SubcategoryId, controlSchemeSubcategory.AllValueId)).ToList();

                var runsAlreadySubmitted = verifiedRuns
                    .Where(x => x.HasVariable(controlSchemeSubcategory.SubcategoryId, controlSchemeSubcategory.TankValueId))
                    .Select(x => x.Comment)
                    .Where(x =>
                    x is not null &&
                    x.Contains("Automatically submitted by TombRunnerBot. Original submission: ")).ToHashSet();

                if (tankRuns.Count == runsAlreadySubmitted.Count)
                {
                    continue;
                }

                foreach (var run in tankRuns)
                {
                    var comment = $"Automatically submitted by TombRunnerBot. Original submission: {run.Weblink}";

                    if (runsAlreadySubmitted.Contains(comment))
                    {
                        continue;
                    }

                    var runCopy = run.ToRun();

                    if (!runCopy.Variables.ContainsKey(controlSchemeSubcategory.SubcategoryId))
                    {
                        continue;
                    }

                    var currentSubcategory = runCopy.Variables[controlSchemeSubcategory.SubcategoryId].Value;

                    runCopy.Variables[controlSchemeSubcategory.SubcategoryId] = new Variable
                    {
                        Type = VariableType.Predefined,
                        Value = currentSubcategory == controlSchemeSubcategory.AllValueId ? controlSchemeSubcategory.TankValueId : controlSchemeSubcategory.AllValueId
                    };

                    runCopy.Verified = true;
                    runCopy.Comment = comment;

                    await _client.Runs.PostRun(runCopy);

                    runsAlreadySubmitted.Add(comment);

                    await Task.Delay(1000, stoppingToken);
                }
            }

            await Task.Delay(60000, stoppingToken);
        }
    }

    private async Task<List<RunModel>> FetchRuns(Game game)
    {
        var allRuns = new List<RunModel>();
        var offset = 0;

        while (true)
        {
            var currentPageRuns = await _client.Runs.GetAllRuns(new RunFilterOptions
            {
                Game = game.Id,
                //Status = RunStatus.Verified, // I think the API is serializing this field incorrectly, so the endpoint returns 404
                OrderBy = "verify-date",
                Direction = SpeedrunDotComAPI.Query.FilterDirection.Desc,
                Max = 200,
                Offset = offset
            });

            allRuns.AddRange(currentPageRuns);

            if (currentPageRuns.Length != 200)
            {
                break;
            }

            offset += 200;
        }

        return allRuns.Where(x => x.Status.Status == RunStatus.Verified).ToList();
    }

}
